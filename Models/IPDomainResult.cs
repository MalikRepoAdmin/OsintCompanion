namespace OsintCompanion.Models
{
    public class IpApiResponse
    {
        // Use full properties for better compatibility and MVVM standards
        public string? Ip { get; set; } 
        public string? Query { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? CountryName { get; set; }
        public string? Country { get; set; }
        public string? Org { get; set; }
        public string? Isp { get; set; }
        public string? Asn { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Timezone { get; set; }

        // Helper to simplify the mapping logic (useful if mapping APIs)
        public static IpApiResponse Normalize(IpApiResponse data)
        {
            if (data == null) return new IpApiResponse();

            data.Ip ??= data.Query;
            data.CountryName ??= data.Country;
            data.Org ??= data.Isp;
            
            return data;
        }
    }
}