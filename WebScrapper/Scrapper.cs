using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebScrapper
{
    internal class Scrapper : IDisposable
    {
        private IWebDriver driver;
        private Configuration config;
        public Scrapper(Configuration config)
        {
            string absolutePathToResults = Path.GetFullPath(config.Settings["path_to_results"]);
            this.config = config;
            try
            {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", absolutePathToResults);
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "ru");
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            driver = new ChromeDriver(chromeOptions);
            }catch (WebDriverException e)
            {
                Console.WriteLine(e.ToString() + "Chrome не найден, использую Edge.");
                var edgeOptions = new EdgeOptions();
                edgeOptions.AddUserProfilePreference("download.default_directory", absolutePathToResults);
                edgeOptions.AddUserProfilePreference("intl.accept_languages", "ru");
                edgeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                driver = new EdgeDriver(edgeOptions);
            }
            
        }

        public void DownloadFiles()
        {
            int secondsToWaitForDownload = 5;
            Login();
            foreach (var pair in config.DistrUrls)
            {
                driver.Navigate().GoToUrl(pair.Value);
                DownloadZipFile();
                string dataPath = config.Settings["path_to_results"] + "\\" + pair.Key + "_" + GetUpdateTime();

                Thread.Sleep(1000 * secondsToWaitForDownload);
                secondsToWaitForDownload = 4;

                var downloadedZips = Directory.EnumerateFiles(config.Settings["path_to_results"]);
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                    ExtractZipsAccordingToDist(dataPath, downloadedZips);
                }
                else
                {
                    foreach (var file in downloadedZips)
                    {
                        if (file.EndsWith(".zip"))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
        }

        private void ExtractZipsAccordingToDist(string dataPath, IEnumerable<string> downloadedZips)
        {
            foreach (var zipFile in downloadedZips)
            {
                if (!zipFile.EndsWith(".zip"))
                {
                    continue;
                }
                string[] pathFolders = zipFile.Split('\\');
                string fileName = pathFolders[pathFolders.Length - 1];
                ZipFile.ExtractToDirectory(zipFile, dataPath + "\\" + fileName.Substring(0, fileName.Length - 4));
                File.Delete(zipFile);
            }
        }

        private void DownloadZipFile()
        {
            var downloadLinks = driver.FindElements(By.LinkText("прайс"));
            foreach (var link in downloadLinks)
            {
                link.Click();
            }
        }
        private void Login()
        {
            driver.Navigate().GoToUrl(config.Settings["login_url"]);
            var loginForm = driver.FindElement(By.Name("usrLogin"));
            var passwordForm = driver.FindElement(By.Name("usrPassword"));
            loginForm.SendKeys(config.Username);
            passwordForm.SendKeys(config.Password);
            loginForm.Submit();
        }
        private string GetUpdateTime() 
        {
            string result = "no_time_info";
            string tableText = driver.FindElement(By.ClassName("border1_100")).Text;
            string[] lines = tableText.Split('\n');
            foreach (var line in lines)
            {
                if (!line.Contains("Обновление прайса:"))
                {
                    continue;
                }
                string[] dateAndTime = line.Split();
                result = dateAndTime[2] + "_" + dateAndTime[3].Remove(dateAndTime[3].Length - 1).Replace(':', '.');
                return result;
            }
            return result;
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }
}
