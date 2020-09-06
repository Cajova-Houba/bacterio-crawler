using System.IO;

namespace BacterioCrawler.BacterioCrawler.Core.Cache
{
    /// <summary>
    /// Saves data for each key to its own html file in given folder.
    /// </summary>
    public class HtmlFileTempDataCache : ITempDataCache
    {
        private readonly string tempFolder;

        public HtmlFileTempDataCache(string tempFolder)
        {
            this.tempFolder = tempFolder;
        }

        public string RetrieveData(string key)
        {
            return File.ReadAllText(GetFilePath(key));
        }

        public void StoreTempData(string key, byte[] data)
        {
            File.WriteAllBytes(GetFilePath(key), data);
        }

        /// <summary>
        /// Creates path to html file from given key.
        /// </summary>
        /// <param name="key">Key to return filepath for.</param>
        /// <returns>Filepath.</returns>
        private string GetFilePath(string key)
        {
            return tempFolder + "/" + key + ".html";
        }
    }
}
