namespace OsintCompanion.Models
{
    /// <summary>
    /// Represents the search result of a username check on a given platform.
    /// </summary>
    public class SocialResult
    {
        public string? Platform { get; set; }  // Platform name (e.g., Twitter)
        public string? Url { get; set; }       // Full URL tested
        public bool Exists { get; set; }       // True if the username exists
        public string? ExtraInfo { get; set; } // Extracted details (emails, meta, etc.)
    }
}
