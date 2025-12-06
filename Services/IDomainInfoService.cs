// IDomainInfoService.cs

using OsintCompanion.Models;

namespace OsintCompanion.Services
{
    // Interface for dependency injection
    public interface IDomainInfoService
    {
        Task<IpApiResponse?> LookupDomainOrIpAsync(string query, string? apiKey = null);
    }
}