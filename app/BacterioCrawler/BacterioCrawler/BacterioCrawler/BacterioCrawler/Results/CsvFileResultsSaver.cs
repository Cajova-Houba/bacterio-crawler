using System.IO;

namespace BacterioCrawler.BacterioCrawler.Results
{
    /// <summary>
    /// Saves gathered results into csv file.
    /// </summary>
    public class CsvFileResultsSaver : IResultsSaver
    {
        /// <summary>
        /// Name of the output file to save results to.
        /// </summary>
        private readonly string outFileName;

        /// <summary>
        /// CSV delimiter
        /// </summary>
        private readonly char inputDelimiter;

        public CsvFileResultsSaver(string outFileName, char inputDelimiter)
        {
            this.outFileName = outFileName;
            this.inputDelimiter = inputDelimiter;
        }

        public void AddSearchResults(string[] lineItems, string[] searchRes)
        {
            string[] lineToWrite = new string[lineItems.Length + searchRes.Length];
            lineItems.CopyTo(lineToWrite, 0);
            searchRes.CopyTo(lineToWrite, lineItems.Length);

            using (var writer = new StreamWriter(outFileName, true))
            {
                foreach (string item in lineToWrite)
                {
                    writer.Write(item + inputDelimiter);
                }
                writer.WriteLine("");
            }
        }
    }
}
