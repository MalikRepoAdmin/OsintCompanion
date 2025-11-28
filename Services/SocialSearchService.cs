using System.Net.Http;
using System.Text.RegularExpressions;
using OsintCompanion.Models;

namespace OsintCompanion.Services
{
    /// <summary>
    /// Service to search a username across multiple social sites.
    /// Supports optional metadata extraction (title, emails, links).
    /// </summary>
    public class SocialSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly List<SiteDefinition> _sites = new();

        /// <summary>
        /// Represents a social site to search.
        /// </summary>
        private record SiteDefinition(
            string Name,
            string Url,
            string? Category = null,
            bool ExtractTitle = false,
            bool ExtractMeta = false,
            bool ExtractEmail = false,
            bool ExtractLinks = false
        );

        public SocialSearchService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            // Set a realistic User-Agent to avoid 403s from some sites
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/120.0 Safari/537.36");

            // Initialize sites (could also load from JSON)
            _sites = new List<SiteDefinition>
            {
                new("Twitter", "https://twitter.com/{username}", "social", true, true, true, true),
                new("GitHub", "https://github.com/{username}", "developer", true, true, true, true),
                new("Reddit", "https://www.reddit.com/user/{username}", "forum", true, true, false, false),
                new("Instagram", "https://instagram.com/{username}", "social", true, true, true, true),
                new("TikTok", "https://www.tiktok.com/@{username}", "social", true, true, true, true),
                new("Pinterest", "https://www.pinterest.com/{username}/", "social", true, true, true, true),
                new("SoundCloud", "https://soundcloud.com/{username}", "music", true, true, true, true),
                new("LinkedIn", "https://LinkedIn.com/in/{username}", "social", true, true, true, true)
            };
        }

        /// <summary>
        /// Searches the given username across all configured sites.
        /// </summary>
        /// <param name="username">The username to search.</param>
        /// <param name="log">Optional logger callback.</param>
        /// <returns>List of SocialResult with existence and metadata info.</returns>
        public async Task<List<SocialResult>> SearchUsernameAsync(string username, Action<string>? log = null)
        {
            List<SocialResult> results = new();

            foreach (var site in _sites)
            {
                string siteUrl = site.Url.Replace("{username}", username);
                log?.Invoke($"🌐 Checking {siteUrl} ...");

                try
                {
                    using var response = await _httpClient.GetAsync(siteUrl);

                    string html = await response.Content.ReadAsStringAsync();

                    // Determine existence:
                    // Either HTTP 2xx success OR username exists in HTML content
                    bool exists = response.IsSuccessStatusCode || html.Contains(username, StringComparison.OrdinalIgnoreCase);

                    string extraInfo = exists ? ExtractInfo(html, site) : "";

                    results.Add(new SocialResult
                    {
                        Platform = site.Name,
                        Url = siteUrl,
                        Exists = exists,
                        ExtraInfo = extraInfo
                    });

                    log?.Invoke(exists
                        ? $"✅ {site.Name}: Found ({(int)response.StatusCode})"
                        : $"❌ {site.Name}: Not found ({(int)response.StatusCode})");
                }
                catch (Exception ex)
                {
                    // Always continue searching remaining sites even if one fails
                    log?.Invoke($"⚠️ {site.Name}: Error → {ex.Message}");

                    results.Add(new SocialResult
                    {
                        Platform = site.Name,
                        Url = siteUrl,
                        Exists = false,
                        ExtraInfo = $"Error: {ex.Message}"
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Extracts optional metadata from the HTML response (title, emails, links).
        /// </summary>
        private string ExtractInfo(string html, SiteDefinition site)
        {
            List<string> info = new();

            if (site.ExtractTitle)
            {
                var titleMatch = Regex.Match(html, @"<title>(.*?)<\/title>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                    info.Add($"Title: {titleMatch.Groups[1].Value}");
            }

            if (site.ExtractEmail)
            {
                var emails = Regex.Matches(html, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-z]{2,}");
                foreach (Match email in emails)
                    info.Add($"Email: {email.Value}");
            }

            if (site.ExtractLinks)
            {
                var links = Regex.Matches(html, @"https?:\/\/[^\s""']+");
                foreach (Match link in links)
                    info.Add($"Link: {link.Value}");
            }

            return string.Join("\n", info.Distinct());
        }
    }
}
