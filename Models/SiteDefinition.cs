namespace OsintCompanion.Models
{
    /// <summary>
    /// Represents a website or platform to check for a given username.
    /// </summary>
    public class SiteDefinition
    {
        public string Name { get; set; }          // Site display name (e.g., "Twitter")
        public string Url { get; set; }           // URL template (e.g., "https://twitter.com/{username}")
        public string Category { get; set; }      // Category (social, forum, etc.)
        public bool ExtractTitle { get; set; }    // Whether to extract <title>
        public bool ExtractMeta { get; set; }     // Whether to extract meta tags
        public bool ExtractEmail { get; set; }    // Whether to find emails
        public bool ExtractLinks { get; set; }    // Whether to find external links

        // ✅ Add a constructor that matches how we initialize this class in the service
        public SiteDefinition(
            string name,
            string url,
            string category,
            bool extractTitle,
            bool extractMeta,
            bool extractEmail,
            bool extractLinks)
        {
            Name = name;
            Url = url;
            Category = category;
            ExtractTitle = extractTitle;
            ExtractMeta = extractMeta;
            ExtractEmail = extractEmail;
            ExtractLinks = extractLinks;
        }

        // Empty constructor for JSON deserialization
        public SiteDefinition() { }
    }
}
