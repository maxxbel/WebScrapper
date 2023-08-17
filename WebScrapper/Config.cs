using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebScrapper
{
    /// <summary>
    /// Singleton class containing the configuration of the programm.
    /// </summary>
    internal class Config
    {
        private const string ConfigPath = "config.txt";
        public Dictionary<string, string> Dict = new Dictionary<string, string>();
        public Dictionary<string, string> GroupUrls = new Dictionary<string, string>();
        private string username = string.Empty;
        private string password = string.Empty;

        private static Config instance;

        private Config()
        {
            Update();
        }
        /// <summary>
        /// Getter for instance of Config class.
        /// </summary>
        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }
        /// <summary>
        /// Reads a text file line by line, ignores lines starting with #. 
        /// </summary>
        /// <param name="path">Valid path to the text file to read.</param>
        /// <returns> Puts the first word in the line as a key, the second as it's value.</returns>
        private static Dictionary<string, string> ReadDictFromFile(string path)
        {
            var result = new Dictionary<string, string>();
            using (StreamReader sr = File.OpenText(path))
            {
                string line = sr.ReadLine();

                // Check if the file is over
                while (!(line is null))
                {
                    // Check if the line is a comment.
                    if (line[0] != '#')
                    {
                        var lines = line.Split();
                        result[lines[0]] = lines[1];
                    }

                    line = sr.ReadLine();
                }
            }
            return result;
        }

        /// <summary>
        /// Checks if a file can be read.
        /// </summary>
        /// <param name="path">Valid path to text file.</param>
        /// <returns>True if a file can be accessed for read, false if not.</returns>
        private static bool IsFileFree(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            // There is no better way to check this, the other solution involves using windows dlls
            // and is slower when everything is OK, which is most of the time.
            try
            {
                using (StreamReader sr = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                {
                    return true;
                }
            }catch (IOException)
            {
                return false;
            }
            
        }

        public void Update()
        {
            // The checks won't be nessesary when the program is
            // rewritten to use GUI
            if (IsFileFree(ConfigPath))
            {
                Dict = ReadDictFromFile(ConfigPath);
            }

            if (IsFileFree(Dict["path_to_credentials"]))
            {
                var credentials = ReadDictFromFile(Dict["path_to_credentials"]);
                password = credentials["password"];
                username = credentials["username"];
            }

            if (IsFileFree(Dict["path_to_distr_urls"]))
            {
                GroupUrls = ReadDictFromFile(Dict["path_to_distr_urls"]);
            }

            if (!Directory.Exists(Dict["path_to_results"] + "\\downloads"))
            {
                Directory.CreateDirectory(Dict["path_to_results"] + "\\downloads");
            }
        }

        /// <summary>
        /// Getter for username.
        /// </summary>
        public string Username
        {
            get { return username; }
        }

        /// <summary>
        /// Getter for password.
        /// </summary>
        public string Password
        {
            get { return password; }
        }
    }
}
