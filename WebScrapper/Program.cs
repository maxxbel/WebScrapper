using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using System.Runtime.InteropServices;


namespace WebScrapper
{
    
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private static System.Timers.Timer timer;
        private static System.Timers.Timer updateConfig;
        static void Main()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            updateConfig = new System.Timers.Timer(60000);
            updateConfig.Elapsed += UpdateConfig;
            updateConfig.AutoReset = true;
            updateConfig.Enabled = true;
            
            double milliseconds = double.Parse(Config.Instance.Dict["hours_between_activations"]) * 3600000;
            timer = new System.Timers.Timer(milliseconds);
            timer.Elapsed += RunProgram;
            timer.AutoReset = true;
            timer.Enabled = true;
            RunProgram(null, null);
            Console.ReadLine();
            timer.Dispose();
            timer.Stop();
        }

        static void RunProgram(object sender, ElapsedEventArgs e)
        {
            double milliseconds = double.Parse(Config.Instance.Dict["hours_between_activations"]) * 3600000;
            timer.Interval = milliseconds;

            var scrapper = new Scrapper();
            scrapper.DownloadFiles();
            Thread.Sleep(3000);
            scrapper.Dispose();
        }

        static void UpdateConfig(object sender, EventArgs e)
        {
            Config.Instance.Update();
            double milliseconds = double.Parse(Config.Instance.Dict["hours_between_activations"]) * 3600000;
            timer.Interval = milliseconds;
        }

    }
}