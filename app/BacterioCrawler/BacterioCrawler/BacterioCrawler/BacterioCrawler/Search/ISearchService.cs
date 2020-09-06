using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacterioCrawler.BacterioCrawler.Search
{
    /// <summary>
    /// Interface for term-search services (e.g. API for some search engine).
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Searches for the term and returns the first link from the results page.
        /// The implementation should make sure that therm is search for only in the Bacterio database.
        /// 
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <returns>First link or null if no results are found.</returns>
        string SearchForTerm(string term);
    }
}
