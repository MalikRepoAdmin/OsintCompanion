// DomainInfoService.cs

using OsintCompanion.Models;
using System.Net.Http.Json;
using System.Net;

namespace OsintCompanion.Services
{
    public class DomainInfoService : IDomainInfoService
    {
        private readonly HttpClient _httpClient = new();
        // Caching logic is part of the service's state
        private readonly Dictionary<string, IpApiResponse> _cache = new(); 

        // Fallback List definition moved to a method for conditional inclusion
        private readonly List<ApiSourceConfig> _baseApiSources;



        public DomainInfoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            // Define the standard, non-key required sources once
            _baseApiSources = new List<ApiSourceConfig>
            {
                // Config for Try 1: ipapi.co (Free/Basic URL)
                new ApiSourceConfig
                {
                    UrlBuilder = (query) => $"https://ipapi.co/{query}/json/",
                    Validator = (response) => response.Ip != null
                },
                // Config for Try 2: ip-api.com (Free URL)
                new ApiSourceConfig
                {
                    UrlBuilder = (query) => $"https://ip-api.com/json/{query}",
                    Validator = (response) => response.Query != null
                },
                // Config for Try 3: apify.org (Free URL)
                new ApiSourceConfig
                {
                    // Note: ipify's lookup often uses a different endpoint/parameter. 
                    // We use a general lookup endpoint pattern here, assuming one exists or 
                    // the service supports it in a standard way.
                    UrlBuilder = (query) => $"https://api.ipify.org?ip={query}&format=json",
                    // Assuming the response has an 'ip' field.
                    Validator = (response) => !string.IsNullOrWhiteSpace(response.Ip)
                },
                // Config for Try 4: geo.apify.org (Require API Key)
                new ApiSourceConfig
                {
                    // geo.ipify requires an API key in the URL, even for basic lookups.
                    // We'll use a placeholder URL here. If you have a free public key, 
                    // replace 'YOUR_PUBLIC_KEY' with it.
                    UrlBuilder = (query) => $"https://geo.ipify.org/api/v2/country,city?apiKey=YOUR_PUBLIC_KEY&ipAddress={query}",
                    // geo.ipify returns the queried IP in a field called 'ip' or similar.
                    Validator = (response) => !string.IsNullOrWhiteSpace(response.Ip)
                }
            };
        }


        private struct ApiSourceConfig
        {
            // Func to build the specific API endpoint URL
            public Func<string, string> UrlBuilder { get; set; }
            
            // Func to check if the deserialized response is valid for this API
            public Func<IpApiResponse, bool> Validator { get; set; }
        }


        public async Task<IpApiResponse?> LookupDomainOrIpAsync(string query, string? apiKey = null)
        {
            query = query.Trim().ToLower();
            if (_cache.ContainsKey(query)) return _cache[query];

            string lookupTarget = query;
            
            List<ApiSourceConfig> activeSources = new List<ApiSourceConfig>();

            // 1. 🚨 If an API key is provided, add the geo.ipify premium provider config first 🚨
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                activeSources.Add(new ApiSourceConfig
                {
                    // 🚨 Target geo.ipify.org using the user's key 🚨
                    UrlBuilder = (q) => $"https://geo.ipify.org/api/v2/country,city?apiKey={apiKey.Trim()}&ipAddress={q}",
                    Validator = (response) => !string.IsNullOrWhiteSpace(response.Ip)
                });
            }
            
            // 2. Add the standard, free fallback sources
            activeSources.AddRange(_baseApiSources);

            // 3. Loop through all active sources (User's key first, then free fallbacks)
            foreach (var config in activeSources)
            {
                try
                {
                    string url = config.UrlBuilder(lookupTarget);
                    var result = await _httpClient.GetFromJsonAsync<IpApiResponse>(url);

                    if (result != null && config.Validator(result))
                    {
                        result = IpApiResponse.Normalize(result);
                        _cache[query] = result;
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"API query failed: {ex.Message}");
                }
            }

            return null;
        }


        // Tries ipapi.co first, then ip-api.com
        private async Task<IpApiResponse?> QueryIpFallbackAsync(string query)
        {
            // Try 1: ipapi.co
            try
            {
                string url = $"https://ipapi.co/{query}/json/";
                var result = await _httpClient.GetFromJsonAsync<IpApiResponse>(url);
                if (result != null && result.Ip != null) return IpApiResponse.Normalize(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ipapi.co error: {ex.Message}");
            }

            // Try 2: ip-api.com
            try
            {
                string url = $"https://ip-api.com/json/{query}";
                var alt = await _httpClient.GetFromJsonAsync<IpApiResponse>(url);
                if (alt != null && alt.Query != null)
                {
                    return IpApiResponse.Normalize(alt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ip-api.com error: {ex.Message}");
            }

            return null;
        }
    }
}