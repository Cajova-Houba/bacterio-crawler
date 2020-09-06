using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacterioCrawler.BacterioCrawler.Results
{
    /// <summary>
    /// Interface for saving search results.
    /// </summary>
    public interface IResultsSaver
    {
        /// <summary>
        /// Appends search results to the output (actual output depends on the implementation).
        /// Saved results should be available after this method finishes so if the implementation is saving data into the file
        /// it must make sure that this method closes the file output.
        /// 
        /// </summary>
        /// <param name="lineItems">One line from the source file.</param>
        /// <param name="searchRes">Found results.</param>
        void AddSearchResults(string[] lineItems, string[] searchRes);
    }
}
