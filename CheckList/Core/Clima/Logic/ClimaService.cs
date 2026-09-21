using CheckList.Core.Clima.Domain;
using CheckList.Core.Clima.Domain.OpenMeteo;
using CheckList.Core.Clima.Domain.OpenMeteo.CheckList.Core.Clima.Domain.OpenMeteo;
using System.Globalization;
using System.Text.Json;

namespace CheckList.Core.Clima.Logic
{
    public interface IClimaService
    {
        Task<ClimaResult> ObtenerClimaAsync(string ciudad, string? provincia, CancellationToken cancellationToken = default);
    }

    public class ClimaService : IClimaService
    {
        private string? _ciudadCacheada;
        private string? _provinciaCacheada;
        private ClimaResult? _climaCacheado;
        private readonly HttpClient _httpClient;

        private const string OpenMeteoGeocodingUrl = "https://geocoding-api.open-meteo.com/v1/search";
        private const string OpenMeteoForecastUrl = "https://api.open-meteo.com/v1/forecast";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ClimaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClimaResult> ObtenerClimaAsync(string ciudad, string? provincia, CancellationToken cancellationToken = default)
        {
            var climaCacheado = ObtenerClimaCacheado(ciudad, provincia);

            if (_climaCacheado != null)
            { 
                return _climaCacheado;
            }

            var ubicacion = await ObtenerUbicacionAsync(ciudad, provincia, cancellationToken);
            var forecast = await ObtenerForecastAsync(ubicacion, cancellationToken);

            var resultado = ConstruirResultado(ubicacion, forecast);

            _ciudadCacheada = ciudad;
            _provinciaCacheada = provincia;
            _climaCacheado = resultado;

            return resultado;
        }

        private async Task<UbicacionClima> ObtenerUbicacionAsync(string ciudad, string? provincia, CancellationToken cancellationToken)
        {
            var nombreBusqueda = ciudad;

            if (!string.IsNullOrWhiteSpace(provincia))
            {
                nombreBusqueda += $", {provincia}";
            }

            var url =
                $"{OpenMeteoGeocodingUrl}" +
                $"?name={Uri.EscapeDataString(nombreBusqueda)}" +
                $"&count=1" +
                $"&language=es" +
                $"&countryCode=AR" +
                $"&format=json";

            using var response = await _httpClient.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var data = await JsonSerializer.DeserializeAsync<OpenMeteoGeocodingResponse>(stream, JsonOptions, cancellationToken);

            var resultado = data?.Results?.FirstOrDefault();

            if (resultado == null)
            {
                throw new InvalidOperationException($"No se pudo encontrar la localidad '{nombreBusqueda}' en Argentina.");
            }

            return new UbicacionClima
            {
                Ciudad = resultado.Name,
                Provincia = resultado.Admin1 ?? provincia ?? string.Empty,
                Pais = resultado.Country,
                Latitud = resultado.Latitude,
                Longitud = resultado.Longitude,
                Timezone = resultado.Timezone
            };
        }

        private async Task<OpenMeteoForecastResponse> ObtenerForecastAsync(UbicacionClima ubicacion, CancellationToken cancellationToken)
        {
            var latitude = ubicacion.Latitud.ToString(CultureInfo.InvariantCulture);
            var longitude = ubicacion.Longitud.ToString(CultureInfo.InvariantCulture);

            var url =
                $"{OpenMeteoForecastUrl}" +
                $"?latitude={latitude}" +
                $"&longitude={longitude}" +
                $"&timezone={Uri.EscapeDataString(ubicacion.Timezone)}" +
                $"&forecast_days=7" +
                $"&current=" +
                "temperature_2m," +
                "apparent_temperature," +
                "relative_humidity_2m," +
                "wind_speed_10m," +
                "wind_gusts_10m," +
                "weather_code" +
                $"&hourly=" +
                "temperature_2m," +
                "apparent_temperature," +
                "relative_humidity_2m," +
                "wind_speed_10m," +
                "wind_gusts_10m," +
                "precipitation_probability," +
                "precipitation," +
                "weather_code" +
                $"&daily=" +
                "temperature_2m_max," +
                "temperature_2m_min," +
                "precipitation_probability_max," +
                "precipitation_sum," +
                "wind_speed_10m_max," +
                "weather_code";

            using var response = await _httpClient.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var data =await JsonSerializer.DeserializeAsync<OpenMeteoForecastResponse>(stream, JsonOptions, cancellationToken);

            if (data == null)
            {
                throw new InvalidOperationException("Open-Meteo devolvió una respuesta vacía.");
            }

            return data;
        }

        private static ClimaResult ConstruirResultado(UbicacionClima ubicacion, OpenMeteoForecastResponse forecast)
        {
            if (forecast.Current == null)
            {
                throw new InvalidOperationException("Open-Meteo no devolvió las condiciones actuales.");
            }

            if (forecast.Hourly == null)
            {
                throw new InvalidOperationException("Open-Meteo no devolvió el pronóstico horario.");
            }

            var actual = forecast.Current;

            var actualClima = new ClimaActual
            {
                Hora = actual.Time,
                Temperatura = actual.Temperature,
                SensacionTermica = actual.ApparentTemperature,
                Humedad = actual.RelativeHumidity,
                Viento = actual.WindSpeed,
                Rafaga = actual.WindGusts,
                CodigoWmo = actual.WeatherCode,
                Icono = ObtenerIcono(actual.WeatherCode),
                Descripcion = ObtenerDescripcion(actual.WeatherCode)
            };

            var horas = new List<PronosticoHora>();

            var h = forecast.Hourly;

            var cantidad = new[]
            {
                h.Time.Count,
                h.Temperature.Count,
                h.ApparentTemperature.Count,
                h.RelativeHumidity.Count,
                h.WindSpeed.Count,
                h.WindGusts.Count,
                h.PrecipitationProbability.Count,
                h.Precipitation.Count,
                h.WeatherCode.Count
            }.Min();

            var indiceActual = 0;

            for (var i = 0; i < cantidad; i++)
            {
                if (h.Time[i] >= actual.Time)
                {
                    indiceActual = i;
                    break;
                }
            }

            var limite = Math.Min(
                indiceActual + 24,
                cantidad);

            for (var i = indiceActual; i < limite; i++)
            {
                horas.Add(new PronosticoHora
                {
                    Hora = h.Time[i],
                    Temperatura = h.Temperature[i],
                    SensacionTermica = h.ApparentTemperature[i],
                    Humedad = (int)Math.Round(h.RelativeHumidity[i]),
                    Viento = h.WindSpeed[i],
                    Rafaga = h.WindGusts[i],
                    ProbabilidadLluvia = h.PrecipitationProbability[i],
                    Precipitacion = h.Precipitation[i],
                    CodigoWmo = h.WeatherCode[i],
                    Icono = ObtenerIcono(h.WeatherCode[i]),
                    Descripcion = ObtenerDescripcion(h.WeatherCode[i])
                });
            }

            var dias = new List<PronosticoDia>();

            if (forecast.Daily != null)
            {
                var d = forecast.Daily;

                var cantidadDias = new[]
                {
                    d.Time.Count,
                    d.TemperatureMax.Count,
                    d.TemperatureMin.Count,
                    d.PrecipitationProbabilityMax.Count,
                    d.PrecipitationSum.Count,
                    d.WindSpeedMax.Count,
                    d.WeatherCode.Count
                }.Min();

                for (var i = 0; i < cantidadDias; i++)
                {
                    dias.Add(new PronosticoDia
                    {
                        Fecha = d.Time[i],
                        TemperaturaMaxima = d.TemperatureMax[i],
                        TemperaturaMinima = d.TemperatureMin[i],
                        ProbabilidadLluvia = d.PrecipitationProbabilityMax[i],
                        Precipitacion = d.PrecipitationSum[i],
                        VientoMaximo = d.WindSpeedMax[i],
                        Icono = ObtenerIcono(d.WeatherCode[i]),
                        Descripcion = ObtenerDescripcion(d.WeatherCode[i])
                    });
                }
            }

            var alertas = GenerarAlertasInteligentes(forecast);

            return new ClimaResult
            {
                Ubicacion = ubicacion,
                Actual = actualClima,
                Horas = horas,
                Dias = dias,
                Alertas = alertas,
                Actualizado = DateTime.Now
            };
        }

        private static List<AlertaClima> GenerarAlertasInteligentes(OpenMeteoForecastResponse forecast)
        {
            var alertas = new List<AlertaClima>();
            var actual = forecast.Current;
            var daily = forecast.Daily;
            var hourly = forecast.Hourly;

            if (actual == null) return alertas;

            var ahora = DateTime.Now;
            var inicioDelDia = ahora.Date;
            var finDelDia = ahora.Date.AddDays(1).AddSeconds(-1);

            // --- Extraer métricas de las próximas 24h desde el índice actual ---
            var rafagaMax = actual.WindGusts;
            var vientoMax = actual.WindSpeed;
            var lluviaAcum = 0.0;
            var probLluviaMax = 0;
            var hayTormenta = actual.WeatherCode is 95 or 96 or 99;
            var hayGranizo = actual.WeatherCode is 96 or 99;
            var hayNiebla = actual.WeatherCode is 45 or 48;

            if (hourly != null)
            {
                var idxActual = 0;
                for (var i = 0; i < hourly.Time.Count; i++)
                {
                    if (hourly.Time[i] >= actual.Time) { idxActual = i; break; }
                }

                var limite = Math.Min(idxActual + 24, hourly.Time.Count);
                for (var i = idxActual; i < limite; i++)
                {
                    rafagaMax = Math.Max(rafagaMax, hourly.WindGusts[i]);
                    vientoMax = Math.Max(vientoMax, hourly.WindSpeed[i]);
                    lluviaAcum += hourly.Precipitation[i];
                    probLluviaMax = Math.Max(probLluviaMax, hourly.PrecipitationProbability[i]);

                    if (hourly.WeatherCode[i] is 95 or 96 or 99) hayTormenta = true;
                    if (hourly.WeatherCode[i] is 96 or 99) hayGranizo = true;
                    if (hourly.WeatherCode[i] is 45 or 48) hayNiebla = true;
                }
            }

            var velViento = Math.Max(rafagaMax, vientoMax);
            var tempMax = daily?.TemperatureMax?.Count > 0 ? daily.TemperatureMax[0] : actual.Temperature;
            var tempMin = daily?.TemperatureMin?.Count > 0 ? daily.TemperatureMin[0] : actual.Temperature;

            if (lluviaAcum == 0 && daily?.PrecipitationSum?.Count > 0)
                lluviaAcum = daily.PrecipitationSum[0];

            // --- 1. VIENTO ---
            if (velViento >= 100)
                alertas.Add(Alerta("Rojo", "Viento Extremo",
                    $"Ráfagas de hasta {Math.Round(velViento)} km/h. Peligro extremo.", inicioDelDia, finDelDia));
            else if (velViento >= 75)
                alertas.Add(Alerta("Naranja", "Viento Fuerte",
                    $"Ráfagas de hasta {Math.Round(velViento)} km/h. Riesgo de daños.", inicioDelDia, finDelDia));
            else if (velViento >= 50)
                alertas.Add(Alerta("Amarillo", "Viento Moderado",
                    $"Ráfagas de hasta {Math.Round(velViento)} km/h.", inicioDelDia, finDelDia));

            // --- 2. TORMENTAS / GRANIZO / LLUVIA ---
            // El granizo ya implica tormenta, se priorizan por severidad
            if (hayGranizo && lluviaAcum >= 50)
                alertas.Add(Alerta("Naranja", "Tormentas con Granizo",
                    $"Tormentas severas con granizo. Acumulado estimado {Math.Round(lluviaAcum)} mm.", inicioDelDia, finDelDia));
            else if (hayGranizo)
                alertas.Add(Alerta("Amarillo", "Granizo",
                    "Tormentas con posible caída de granizo.", inicioDelDia, finDelDia));
            else if (hayTormenta)
                alertas.Add(Alerta("Amarillo", "Tormentas",
                    "Tormentas con actividad eléctrica y posibles ráfagas.", inicioDelDia, finDelDia));

            // Lluvia intensa independiente de tormenta
            if (lluviaAcum >= 100)
                alertas.Add(Alerta("Rojo", "Lluvias Extremas",
                    $"Acumulado estimado {Math.Round(lluviaAcum)} mm. Riesgo de inundaciones.", inicioDelDia, finDelDia));
            else if (lluviaAcum >= 50)
                alertas.Add(Alerta("Naranja", "Lluvias Intensas",
                    $"Acumulado estimado {Math.Round(lluviaAcum)} mm. Posibles anegamientos.", inicioDelDia, finDelDia));
            else if (lluviaAcum >= 20 || probLluviaMax >= 60)
                alertas.Add(Alerta("Amarillo", "Lluvias",
                    $"Lluvias esperadas ({Math.Round(lluviaAcum)} mm acum., prob. máx. {probLluviaMax}%).", inicioDelDia, finDelDia));

            // --- 3. CALOR ---
            if (tempMax >= 44)
                alertas.Add(Alerta("Rojo", "Calor Extremo",
                    $"Máxima de {Math.Round(tempMax)}°C. Riesgo muy alto para la salud.", inicioDelDia, finDelDia));
            else if (tempMax >= 40)
                alertas.Add(Alerta("Naranja", "Calor Intenso",
                    $"Máxima de {Math.Round(tempMax)}°C. Peligroso especialmente para grupos de riesgo.", inicioDelDia, finDelDia));
            else if (tempMax >= 35)
                alertas.Add(Alerta("Amarillo", "Altas Temperaturas",
                    $"Máxima de {Math.Round(tempMax)}°C. Mantenerse hidratado.", inicioDelDia, finDelDia));

            // --- 4. FRÍO / HELADAS ---
            if (tempMin <= -10)
                alertas.Add(Alerta("Rojo", "Frío Extremo",
                    $"Mínima de {Math.Round(tempMin)}°C. Peligro extremo para personas y cañerías.", inicioDelDia, finDelDia));
            else if (tempMin <= -5)
                alertas.Add(Alerta("Naranja", "Helada Severa",
                    $"Mínima de {Math.Round(tempMin)}°C. Riesgo alto para cultivos y cañerías.", inicioDelDia, finDelDia));
            else if (tempMin <= 0)
                alertas.Add(Alerta("Amarillo", "Helada",
                    $"Mínima de {Math.Round(tempMin)}°C. Precaución en zonas expuestas.", inicioDelDia, finDelDia));

            // --- 5. NIEBLA ---
            if (hayNiebla)
                alertas.Add(Alerta("Amarillo", "Niebla",
                    "Visibilidad reducida. Precaución al conducir.", inicioDelDia, finDelDia));

            return alertas;
        }

        private static AlertaClima Alerta(string nivel, string fenomeno, string descripcion, DateTime inicio, DateTime fin) =>
            new() { Nivel = nivel, Fenomeno = fenomeno, Descripcion = descripcion, Inicio = inicio, Fin = fin };

        private static (string Icono, string Descripcion) ObtenerWmo(int code)
        {
            return code switch
            {
                0 => ("☀️", "Despejado"),

                1 => ("🌤️", "Mayormente despejado"),
                2 => ("⛅", "Parcialmente nublado"),
                3 => ("☁️", "Nublado"),

                45 => ("🌫️", "Niebla"),
                48 => ("🌫️", "Niebla con escarcha"),

                51 => ("🌦️", "Llovizna leve"),
                53 => ("🌦️", "Llovizna"),
                55 => ("🌧️", "Llovizna intensa"),

                56 => ("🌧️", "Llovizna helada leve"),
                57 => ("🌧️", "Llovizna helada intensa"),

                61 => ("🌧️", "Lluvia leve"),
                63 => ("🌧️", "Lluvia"),
                65 => ("🌧️", "Lluvia intensa"),

                66 => ("🌧️", "Lluvia helada leve"),
                67 => ("🌧️", "Lluvia helada intensa"),

                71 => ("🌨️", "Nevada leve"),
                73 => ("🌨️", "Nevada"),
                75 => ("❄️", "Nevada intensa"),

                77 => ("❄️", "Granos de nieve"),

                80 => ("🌦️", "Chubascos leves"),
                81 => ("🌧️", "Chubascos"),
                82 => ("🌧️", "Chubascos fuertes"),

                85 => ("🌨️", "Chubascos de nieve leves"),
                86 => ("❄️", "Chubascos de nieve fuertes"),

                95 => ("⛈️", "Tormenta"),

                96 => ("⛈️", "Tormenta con granizo"),
                99 => ("⛈️", "Tormenta severa con granizo"),

                _ => ("🌡️", "Desconocido")
            };
        }

        private static string ObtenerIcono(int code)
        {
            return ObtenerWmo(code).Icono;
        }

        private static string ObtenerDescripcion(int code)
        {
            return ObtenerWmo(code).Descripcion;
        }

        private ClimaResult? ObtenerClimaCacheado(string ciudad, string? provincia)
        {
            if (_climaCacheado != null)
            {
                if (!string.Equals(_ciudadCacheada, ciudad, StringComparison.OrdinalIgnoreCase) || !string.Equals(_provinciaCacheada, provincia, StringComparison.OrdinalIgnoreCase))
                {
                    return null;                   
                }

                if(DateTime.Now - _climaCacheado.Actualizado > TimeSpan.FromHours(1))
                {
                    return null;
                }
            }

            return _climaCacheado;
        }
    }

}