using System.Text.Json.Serialization;

namespace CheckList.Core.Clima.Domain.OpenMeteo
{

    namespace CheckList.Core.Clima.Domain.OpenMeteo
    {
        public class OpenMeteoGeocodingResponse
        {
            [JsonPropertyName("results")]
            public List<OpenMeteoLocation>? Results { get; set; }
        }

        public class OpenMeteoLocation
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }

            [JsonPropertyName("timezone")]
            public string Timezone { get; set; } = string.Empty;

            [JsonPropertyName("country")]
            public string Country { get; set; } = string.Empty;

            [JsonPropertyName("country_code")]
            public string CountryCode { get; set; } = string.Empty;

            [JsonPropertyName("admin1")]
            public string? Admin1 { get; set; }

            [JsonPropertyName("admin2")]
            public string? Admin2 { get; set; }
        }
    }
}
