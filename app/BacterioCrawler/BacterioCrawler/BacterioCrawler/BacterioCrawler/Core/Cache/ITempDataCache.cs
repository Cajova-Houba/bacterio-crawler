using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacterioCrawler.BacterioCrawler.Core.Cache
{
    /// <summary>
    /// Interface for storing and retrieving temp data.
    /// </summary>
    public interface ITempDataCache
    {
        /// <summary>
        /// Stores given data under the key.
        /// </summary>
        /// <param name="key">Key to retrieve data. If such key already exists, the data will be overwritten.</param>
        /// <param name="data">Data to save.</param>
        void StoreTempData(string key, byte[] data);

        /// <summary>
        /// Retrieves data for given key.
        /// </summary>
        /// <param name="key">Key used to store data.</param>
        /// <returns>Data or null if such key is not stored.</returns>
        string RetrieveData(string key);
    }
}
