using BacterioCrawler.Search.Google;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using System;

namespace BacterioCrawler.BacterioCrawler.Search.Google
{
    /// <summary>
    /// Wrapper for google's search API.
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly GoogleConfiguration configuration;

        public SearchService(GoogleConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string SearchForTerm(string term)
        {
            var searchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = configuration.key });
            var listRequest = searchService.Cse.Siterestrict.List(term);
            listRequest.Cx = configuration.cx;
            var search = listRequest.Execute();

            if (search.Items != null && search.Items.Count > 0)
            {
                return search.Items[0].Link;
            }
            else
            {
                return null;
            }
        }
    }
}
