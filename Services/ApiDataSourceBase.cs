// Services/ApiDataSourceBase.cs

using OsintCompanion.Models;
using System.Net.Http.Json;

namespace OsintCompanion.Services
{
    // Abstract class to define the common API fetching strategy
    public abstract class ApiDataSourceBase
    {
        protected readonly HttpClient HttpClient;

        // Constructor to inject the shared HttpClient
        public ApiDataSourceBase(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        // 1. ABSTRACT: Must be implemented by derived classes to construct the specific API URL
        protected abstract string BuildUrl(string query);
        
        // 2. ABSTRACT: Must be implemented to check if the response is valid (since APIs differ)
        protected abstract bool IsValidResponse(IpApiResponse? response);
        
        // 3. ABSTRACT: Must be implemented to set any unique properties/normalization if needed
        // We can optionally merge this with the Model's Normalize method, but keeping it here
        // allows for API-specific transformations before final Model normalization.
        protected abstract IpApiResponse NormalizeApiSpecifics(IpApiResponse response);

        // 4. CONCRETE: The shared logic (fetch, handle exception, and check)
        public async Task<IpApiResponse?> QueryAsync(string query)
        {
            try
            {
                string url = BuildUrl(query);
                var result = await HttpClient.GetFromJsonAsync<IpApiResponse>(url);

                if (result != null && IsValidResponse(result))
                {
                    // Apply API-specific normalization before returning
                    return NormalizeApiSpecifics(result);
                }
            }
            catch (Exception ex)
            {
                // We log the error here but allow the fallback to continue by returning null
                Console.WriteLine($"Error querying {this.GetType().Name}: {ex.Message}");
            }

            return null;
        }
    }
}