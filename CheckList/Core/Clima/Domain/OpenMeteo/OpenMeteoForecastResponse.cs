using System.Text.Json.Serialization;

namespace CheckList.Core.Clima.Domain.OpenMeteo
{
    public class OpenMeteoForecastResponse
    {
        [JsonPropertyName("timezone")]
        public string Timezone { get; set; } = string.Empty;

        [JsonPropertyName("current")]
        public OpenMeteoCurrent? Current { get; set; }

        [JsonPropertyName("hourly")]
        public OpenMeteoHourly? Hourly { get; set; }

        [JsonPropertyName("daily")]
        public OpenMeteoDaily? Daily { get; set; }
    }

    public class OpenMeteoCurrent
    {
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("apparent_temperature")]
        public double ApparentTemperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double RelativeHumidity { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_gusts_10m")]
        public double WindGusts { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }
    }

    public class OpenMeteoHourly
    {
        [JsonPropertyName("time")]
        public List<DateTime> Time { get; set; } = [];

        [JsonPropertyName("temperature_2m")]
        public List<double> Temperature { get; set; } = [];

        [JsonPropertyName("apparent_temperature")]
        public List<double> ApparentTemperature { get; set; } = [];

        [JsonPropertyName("relative_humidity_2m")]
        public List<double> RelativeHumidity { get; set; } = [];

        [JsonPropertyName("wind_speed_10m")]
        public List<double> WindSpeed { get; set; } = [];

        [JsonPropertyName("wind_gusts_10m")]
        public List<double> WindGusts { get; set; } = [];

        [JsonPropertyName("precipitation_probability")]
        public List<int> PrecipitationProbability { get; set; } = [];

        [JsonPropertyName("precipitation")]
        public List<double> Precipitation { get; set; } = [];

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; } = [];
    }

    public class OpenMeteoDaily
    {
        [JsonPropertyName("time")]
        public List<DateTime> Time { get; set; } = [];

        [JsonPropertyName("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; } = [];

        [JsonPropertyName("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; } = [];

        [JsonPropertyName("precipitation_probability_max")]
        public List<int> PrecipitationProbabilityMax { get; set; } = [];

        [JsonPropertyName("precipitation_sum")]
        public List<double> PrecipitationSum { get; set; } = [];

        [JsonPropertyName("wind_speed_10m_max")]
        public List<double> WindSpeedMax { get; set; } = [];

        [JsonPropertyName("weather_code")]
        public List<int> WeatherCode { get; set; } = [];
    }
}