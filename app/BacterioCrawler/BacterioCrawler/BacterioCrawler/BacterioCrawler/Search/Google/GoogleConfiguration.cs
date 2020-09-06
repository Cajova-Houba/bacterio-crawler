namespace BacterioCrawler.Search.Google
{
    /// <summary>
    /// Message-pattern like class for storing google-related config.
    /// </summary>
    public class GoogleConfiguration
    {
        public readonly string cx;

        public readonly string key;

        public GoogleConfiguration(string cx, string key)
        {
            this.cx = cx;
            this.key = key;
        }
    }
}
