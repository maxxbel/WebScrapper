using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
        private Config config;
        public Scrapper()
        {
            this.config = Config.Instance;
            string absolutePathToDownload = Path.GetFullPath(config.Dict["path_to_results"] + "\\downloads");
            try
            {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("download.default_directory", absolutePathToDownload);
            chromeOptions.AddUserProfilePreference("intl.accept_languages", "ru");
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            driver = new ChromeDriver(chromeOptions);
            }catch (Exception e)
            {
                Console.WriteLine(e.ToString() + "Chrome не найден, использую Edge.");
                var edgeOptions = new EdgeOptions();
                edgeOptions.AddUserProfilePreference("download.default_directory", absolutePathToDownload);
                edgeOptions.AddUserProfilePreference("intl.accept_languages", "ru");
                edgeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                driver = new EdgeDriver(edgeOptions);
            }
            
        }

        public void DownloadFiles()
        {
            int secondsToWaitForDownload = 6;
            Login();

            foreach (var pair in config.GroupUrls)
            {
                driver.Navigate().GoToUrl(pair.Value);
                var downloadLinks = driver.FindElements(By.LinkText("прайс"));
                string[] dates = GetStringUpdateTimeFromPage(downloadLinks.Count());
                int i = 0;
                foreach ( var link in downloadLinks)
                {
                    string uniqueName;
                    if (downloadLinks.Count() == 1)
                    {
                        uniqueName = config.Dict["path_to_results"] + '\\' + pair.Key + '_' + dates[i] + ".xls";
                    }
                    else
                    {
                        uniqueName = config.Dict["path_to_results"] + '\\' + pair.Key + '_' + dates[i] + $"_part{i + 1}.xls";
                    }
                    
                    i++;

                    if (File.Exists(uniqueName))
                    {
                        continue;
                    }

                    link.Click();
                    Thread.Sleep(secondsToWaitForDownload * 1000);
                    secondsToWaitForDownload = 5;

                    var downloadedZips = Directory.EnumerateFiles(config.Dict["path_to_results"] + "\\downloads");
                    foreach (var zipFile in downloadedZips)
                    {
                        if (!zipFile.EndsWith(".zip"))
                        {
                            continue;
                        }
                        ZipFile.ExtractToDirectory(zipFile, config.Dict["path_to_results"] + "\\downloads");
                        File.Delete(zipFile);
                    }
                    var extractedFile = Directory.GetFiles(config.Dict["path_to_results"] + "\\downloads");
                    File.Copy(extractedFile[0], uniqueName, true);
                    File.Delete(extractedFile[0]);
                }
            }
            try
            {
                CopyResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        private void CopyResults()
        {
            var destination = new DirectoryInfo(config.Dict["path_to_results_copy"]);
            var sourse = new DirectoryInfo(config.Dict["path_to_results"]);
            
            var sourseFiles = sourse.GetFiles();
            if (destination.Exists)
            {
                var filesAlreadyThere = destination.GetFiles();
                foreach (var oldFile in sourseFiles)
                {
                    bool isThere = false;
                    foreach (var file in filesAlreadyThere)
                    {
                        if (oldFile.Name.Equals(file.Name))
                        {
                            isThere = true;
                            break;
                        }
                    }
                    if (!isThere)
                    {
                        oldFile.CopyTo(destination.FullName + '\\' + oldFile.Name);
                    }
                }
            }
            else
            {
                destination.Create();
                foreach (var file in sourseFiles)
                {
                    file.CopyTo(destination.FullName + '\\' + file.Name);
                }
            }
        }

        private string[] GetStringUpdateTimeFromPage(int countOfFiles)
        {
            string[] result = new string[countOfFiles];
            result[0] = "no_time_info";
            string tableText = driver.FindElement(By.ClassName("border1_100")).Text;
            string[] lines = tableText.Split('\n');
            foreach (var line in lines)
            {
                if (!line.Contains("Обновление прайса:"))
                {
                    continue;
                }
                string[] dateAndTime = line.Split();
                if (countOfFiles == 1)
                {
                    string date = dateAndTime[2];
                    string time = dateAndTime[3].Remove(dateAndTime[3].Length - 1).Replace(':', '.');
                    result[0] = date + "_" + time;
                    return result;
                }
                for (int i = 0; i < countOfFiles; i++)
                {
                    string date = dateAndTime[4 + 2 * i];
                    string time = dateAndTime[5 + 2 * i].Remove(dateAndTime[5 + 2 * i].Length - 1).Replace(':', '.');
                    result[i] = date + "_" + time;
                }
            }
            return result;
        }

        private void Login()
        {
            driver.Navigate().GoToUrl(config.Dict["login_url"]);
            var loginForm = driver.FindElement(By.Name("usrLogin"));
            var passwordForm = driver.FindElement(By.Name("usrPassword"));
            loginForm.SendKeys(config.Username);
            passwordForm.SendKeys(config.Password);
            loginForm.Submit();
        }
        public void Dispose()
        {
            driver.Quit();
        }
    }
}
