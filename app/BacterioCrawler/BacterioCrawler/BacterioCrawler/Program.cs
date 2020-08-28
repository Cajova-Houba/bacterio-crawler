using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace BacterioCrawler
{
    /// <summary>
    /// Message-like class for storing google-related config.
    /// </summary>
    class GoogleConfiguration
    {
        public readonly string cx;

        public readonly string key;

        public GoogleConfiguration(string cx, string key)
        {
            this.cx = cx;
            this.key = key;
        }
    }

    class Program
    {
        private static readonly string KEYWORD_FILE_NAME = "keywords.csv";
        private static readonly string OUT_FILE_NAME = "results.csv";
        private static readonly string TEMP_FOLDER = "html";


        static void Main(string[] args)
        {
            Console.WriteLine("Yo, the Bacterio Crawler is startin' baby.");

            string inputFileName = GetInputFileName();
            if (inputFileName == null)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            string[] sourceLines = LoadFileByLines(inputFileName);
            Dictionary<string, string[]> keywords = LoadKeywords(KEYWORD_FILE_NAME);
            GoogleConfiguration googleConfiguration = LoadGoogleConfiguration();

            if (sourceLines != null && sourceLines.Length > 0 && keywords != null && googleConfiguration != null)
            {
                try
                {
                    PrepareTmpHtmlFolder(TEMP_FOLDER);

                    PrepareOutFile(OUT_FILE_NAME, keywords);

                    DoSearch(sourceLines, keywords, googleConfiguration);

                    Console.WriteLine("Done.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected exception: {0}.", ex.Message);
                    Console.WriteLine("Cause: {0}.", ex.Source);
                    Console.WriteLine("Stack trace: {0}.", ex.StackTrace);
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Tries to get input file name from application config.
        /// </summary>
        /// <returns></returns>
        private static string GetInputFileName()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    Console.WriteLine("Missing configuration, can't load input file name.");
                    return null;
                }

                if (appSettings["inputFile"] == null)
                {
                    Console.WriteLine("Missing 'inputFile' field in configuration.");
                    return null;
                }

                return appSettings["inputFile"];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception: {0}.", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates empty folder for temp results. Cleans existing one.
        /// 
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        private static void PrepareTmpHtmlFolder(string folderName)
        {
            Console.WriteLine("Preparing temp folder '{0}'.", folderName);

            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            Directory.CreateDirectory(folderName);
        }

        private static void DoSearch(string[] sourceLines, Dictionary<string, string[]> keywords, GoogleConfiguration googleConfiguration)
        {
            Console.WriteLine("Performing search.");
            BacterioSearcher bacterioSearcher = new BacterioSearcher(googleConfiguration, keywords, OUT_FILE_NAME, TEMP_FOLDER);
            bacterioSearcher.DoSearch(sourceLines);
        }

        /// <summary>
        /// initializes (and removes old one) output file.
        /// </summary>
        /// <param name="fileName">Name of the output file.</param>
        /// <param name="keywords">Keywords to create headers in the output file.</param>
        private static void PrepareOutFile(string fileName, Dictionary<string, string[]> keywords)
        {
            Console.WriteLine("Preparing output file {0}.", fileName);

            // CID_1372640;2.0;k__Bacteria; p__Proteobacteria; c__Alphaproteobacteria; o__Sphingomonadales; f__Sphingomonadaceae; g__Sphingomonas; s__
            List<string> headerLine = new List<string>(new string[] { "OTU ID", "S1", "k", "p", "c", "o", "f", "g", "s" });
            
            foreach (string keyword in keywords.Keys)
            {
                headerLine.Add(keyword);
            }

            using (var writer = new StreamWriter(fileName, false))
            {
                foreach(string item in headerLine)
                {
                    writer.Write(item + ';');
                }
                writer.WriteLine("");
            }
        }

        /// <summary>
        /// Loads google configuration from App.config file.
        /// </summary>
        /// <returns>Configuration or null if error occurs.</returns>
        static GoogleConfiguration LoadGoogleConfiguration()
        {
            Console.WriteLine("Loading app configuration.");

            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    Console.WriteLine("Missing configuration, can't load google details.");
                    return null;
                }

                if (appSettings["googleKey"] == null)
                {
                    Console.WriteLine("Missing 'googleKey' field in configuration.");
                    return null;
                }

                if (appSettings["cx"] == null)
                {
                    Console.WriteLine("Missing 'cx' field in configuration.");
                    return null;
                }

                return new GoogleConfiguration(appSettings["cx"], appSettings["googleKey"]);
            } catch (Exception ex)
            {
                Console.WriteLine("Unexpected exception: {0}.", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Loads all lines from given file.
        /// </summary>
        /// <param name="fileName">File to read from.</param>
        /// <returns>Read lines.</returns>
        static string[] LoadFileByLines(string fileName)
        {
            Console.WriteLine("Loading input from {0}.", fileName);

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File " + fileName + " not found.");
                return new string[0];
            } else
            {
                return File.ReadAllLines(fileName);
            }
        }

        /// <summary>
        /// Loads keywords from CSV file.
        /// </summary>
        /// <param name="fileName">File to load keywords from.</param>
        /// <returns>Keywords in format: keyword -> it's negatives.</returns>
        static Dictionary<string, string[]> LoadKeywords(string fileName)
        {
            Dictionary<string, string[]> res = new Dictionary<string, string[]>();

            Console.WriteLine("Loading keywords from file {0}.", fileName);

            try
            {
                string[] keywordLines = File.ReadAllLines(fileName);
                foreach (string keywordLine in keywordLines)
                {
                    string[] keywordItems = keywordLine.Split(';');
                    List<string> negatives = new List<string>();
                    for (int i = 1; i < keywordItems.Length; i++)
                    {
                        negatives.Add(keywordItems[i]);
                    }

                    res[keywordItems[0]] = negatives.ToArray();
                }

                return res;
            } catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: {0}.", ex.Message);
                return null;
            }
        }
    }
}
