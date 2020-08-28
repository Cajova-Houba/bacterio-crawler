﻿using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace BacterioCrawler
{
    class Parser
    {
        /// <summary>
        /// Regex for identifying empty items in one line of source file.
        /// </summary>
        private static readonly Regex emptyItemRegex = new Regex(" [pcofgs]__[\n]?$", RegexOptions.Compiled);


        /// <summary>
        /// Regex for identifying items in one line of source file.
        /// </summary>
        private static readonly Regex itemRegex = new Regex("[kpcofgs]__([a-zA-Z]+)", RegexOptions.Compiled);

        /// <summary>
        /// Checks whether the given item is emptyItem or not.
        /// </summary>
        /// <param name="item">One item from line from source file.</param>
        /// <returns>True if the item is empty (has no contents besides leading 'letter'__.</letter></returns>
        public static bool IsEmptyItem(string item)
        {
            return item.Length == 0 || emptyItemRegex.IsMatch(item);
        }

        /// <summary>
        /// Parse search term from line item.
        /// </summary>
        /// <param name="item">Non empty line item.</param>
        /// <returns>Search term nor null if no is found.</returns>
        public static string ParseTermFromLineItem(string item)
        {
            MatchCollection matches = itemRegex.Matches(item);
            if (matches.Count > 0 && matches[0].Groups.Count > 1)
            {
                return matches[0].Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        public static string[] ParseLine(string line)
        {
            return line.Split(';');
        }

    }

    class BacterioSearcher
    {
        private static readonly char COMMENT_CHAR = '#';

        /// <summary>
        /// Add this to the search result if the positive form of keyword was
        /// found in the content.
        /// </summary>
        private static readonly string KEYWORD_POSITIVE = "1";

        /// <summary>
        /// Add this to the search result if the negative form of keyword
        /// was found in the content (overrides the positive form).
        /// </summary>
        private static readonly string KEYWORD_NEGATIVE = "0";

        /// <summary>
        /// Add this to the search result if the keyword was not found 
        /// in the content.
        /// </summary>
        private static readonly string KEYWORD_NOT_FOUND = "-";

        private readonly GoogleConfiguration googleConfiguration;

        private readonly Dictionary<string, string[]> keywords;

        private readonly string outFileName;

        private readonly string tempFolder;

        public BacterioSearcher(GoogleConfiguration googleConfiguration, Dictionary<string, string[]> keywords, string outFileName, string tempFolder)
        {
            this.googleConfiguration = googleConfiguration;
            this.keywords = keywords;
            this.outFileName = outFileName;
            this.tempFolder = tempFolder;
        }

        public void DoSearch(string[] sourceLines)
        {
            // term -> results
            Dictionary<string, string[]> searchCache = new Dictionary<string, string[]>();

            int linesProcessed = 0;
            int nextProgMsg = 0;

            foreach (string line in sourceLines)
            {
                int progress = 100*linesProcessed / sourceLines.Length;

                if (nextProgMsg <= progress) 
                {
                    Console.WriteLine("Progess: {0}%.", nextProgMsg);
                    nextProgMsg += 10;
                }

                if (line[0] != COMMENT_CHAR)
                {
                    // get line as separate items
                    string[] lineItems = ProcessSourceLine(line);

                    // pick the right term to search from the whole line
                    string term = PickTermFromLine(lineItems);

                    // either get results from searchMap (kind of a cache)
                    // or find it on bacterio
                    string[] searchRes;
                    if (!searchCache.ContainsKey(term))
                    {
                        // search basterio
                        searchRes = SearchForTerm(term, keywords);
                        searchCache[term] = searchRes;
                    }
                    else
                    {
                        searchRes = searchCache[term];
                    }

                    SaveSearchResult(lineItems, searchRes);
                }

                linesProcessed++;
            }
        }

        /// <summary>
        /// Appends one line containing the search results into out file.
        /// </summary>
        /// <param name="lineItems">Line from source file.</param>
        /// <param name="searchRes">Search results for one term.</param>
        private void SaveSearchResult(string[] lineItems, string[] searchRes)
        {
            string[] lineToWrite = new string[lineItems.Length + searchRes.Length];
            lineItems.CopyTo(lineToWrite, 0);
            searchRes.CopyTo(lineToWrite, lineItems.Length);

            using (var writer = new StreamWriter(outFileName, true))
            {
                foreach (string item in lineToWrite)
                {
                    writer.Write(item + ';');
                }
                writer.WriteLine("");
            }
        }

        /// <summary>
        /// Performs search for given term.
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <param name="keywords">Keywords to search for in found page.</param>
        /// <returns>List representing occurrence of keywords.</returns>
        private string[] SearchForTerm(string term, Dictionary<string, string[]> keywords)
        {
            string pageLink = SearchTerm(term);
            if (pageLink == null)
            {
                Console.Write("No link for '{0}' found.", term);
                return new string[0];
            }

            DownloadPage(pageLink, term);
            return SearchForKeywords(term, keywords);
        }

        /// <summary>
        /// Performs search for keywords in a page downloaded into file called html/[term].html.
        /// </summary>
        /// <param name="term">Term used to locate downloaded page.</param>
        /// <param name="keywords">Keywords to look for in the file.</param>
        /// <returns>Search results.</returns>
        private string[] SearchForKeywords(string term, Dictionary<string, string[]> keywords)
        {
            List<string> res = new List<string>();
            string content = File.ReadAllText(tempFolder +"/" + term + ".html");
            
            foreach(string keyword in keywords.Keys)
            {
                // search for negative
                bool isNegative = false;
                foreach(string negativeKeyword in keywords[keyword])
                {
                    if (content.Contains(negativeKeyword))
                    {
                        res.Add(KEYWORD_NEGATIVE);
                        isNegative = true;
                        break;
                    }
                }

                // no negative, try positive
                if (!isNegative)
                {
                    if (content.Contains(keyword))
                    {
                        res.Add(KEYWORD_POSITIVE);
                    } else
                    {
                        res.Add(KEYWORD_NOT_FOUND);
                    }
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// Downloads page given by link and saves it to file named html/[term].html.
        /// </summary>
        /// <param name="pageLink">Link to page to download.</param>
        /// <param name="term">Term used to name the page.</param>
        private void DownloadPage(string pageLink, string term)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(pageLink, tempFolder + "/" +term+".html");
            }
        }

        /// <summary>
        /// This method searches bacterio database and returns link to 
        /// the first page returned by search engine.
        /// </summary>
        /// <param name="term">Term to search for.</param>
        /// <returns>Link to bacterio page with given term.</returns>
        private string SearchTerm(string term)
        {
            var searchService = new CustomsearchService(new BaseClientService.Initializer { ApiKey = googleConfiguration.key });
            var listRequest = searchService.Cse.Siterestrict.List(term);
            listRequest.Cx = googleConfiguration.cx;
            var search = listRequest.Execute();

            if (search.Items != null && search.Items.Count > 0)
            {
                return search.Items[0].Link;
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Picks term to search for from line from source file.
        /// </summary>
        /// <param name="lineItems">Line split into items.</param>
        /// <returns>Term to search for (or null).</returns>
        private string PickTermFromLine(string[] lineItems)
        {
            foreach (string item in lineItems.Reverse())
            {
                if (!Parser.IsEmptyItem(item))
                {
                    return Parser.ParseTermFromLineItem(item);
                }
            }

            return null;
        }

        /// <summary>
        /// Splits line from source file and returns it as a list of items.
        /// </summary>
        /// <param name="line">One line from source file.</param>
        /// <returns>Line split to items.</returns>
        private string[] ProcessSourceLine(string line)
        {
            return Parser.ParseLine(line);
        }
    }
}