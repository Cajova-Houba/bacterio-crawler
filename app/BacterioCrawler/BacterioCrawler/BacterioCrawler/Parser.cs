using System.Text.RegularExpressions;

namespace BacterioCrawler
{
    public class Parser
    {
        /// <summary>
        /// Regex for identifying empty items in one line of source file.
        /// </summary>
        private static readonly Regex emptyItemRegex = new Regex("[ ]*[pcofgs]__[\n]?$", RegexOptions.Compiled);


        /// <summary>
        /// Regex for identifying items in one line of source file.
        /// </summary>
        private static readonly Regex itemRegex = new Regex("[kpcofgs]__([a-zA-Z]+)", RegexOptions.Compiled);

        private readonly char inputDelimiter;

        public Parser(char inputDelimiter)
        {
            this.inputDelimiter = inputDelimiter;
        }

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

        public string[] ParseLine(string line)
        {
            return line.Split(inputDelimiter);
        }

    }
}
