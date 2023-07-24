using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebScrapper
{
    
    internal class Configuration
    {
        private const string ConfigPath = "config.txt";
        public Dictionary<string, string> Settings = new Dictionary<string, string>();
        public Dictionary<string, string> DistrUrls = new Dictionary<string, string>();
        private string username = string.Empty;
        private string password = string.Empty;

        private Dictionary<string, string> ReadConfigFromFile(string path)
        {
            var result = new Dictionary<string, string>();
            using (StreamReader sr = File.OpenText(path))
            {
                string line = sr.ReadLine();
                while (!(line is null))
                {
                    if (line[0] != '#')
                    {
                        var lines = line.Split();
                        result[lines[0]] = lines[1];
                    }

                    line = sr.ReadLine();
                    Console.WriteLine(line);
                }
            }
            return result;
        }
        private bool IsFileFree(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            try
            {
                using (StreamReader sr = new StreamReader(File.Open(path, FileMode.Open, FileAccess.ReadWrite)))
                {
                    return true;
                }
            }catch (IOException)
            {
                return false;
            }
            
        }
        public Configuration()
        {
            Update();
        }
        public void Update()
        {
            if (!IsFileFree(ConfigPath))
            {
                return;
            }
            Settings = ReadConfigFromFile(ConfigPath);
            if (!IsFileFree(Settings["path_to_credentials"]))
            {
                return;
            }
            var credentials = ReadConfigFromFile(Settings["path_to_credentials"]);
            password = credentials["password"];
            username = credentials["username"];
            if (!IsFileFree(Settings["path_to_distr_urls"]))
            {
                return;
            }
            DistrUrls = ReadConfigFromFile(Settings["path_to_distr_urls"]);
            if (!Directory.Exists(Settings["path_to_results"]))
            {
                Directory.CreateDirectory(Settings["path_to_results"]);
            }
        }
        public string Username
        {
            get { return username; }
        }
        public string Password
        {
            get { return password; }
        }
    }
}
