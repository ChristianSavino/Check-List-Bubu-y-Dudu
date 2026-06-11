using System.Text.Json;

namespace CheckList.Core.Tarea.Logic
{
    public interface IFeriadoService
    {
        Dictionary<string, string> GetFeriados(int año);
        Task CargarFeriadosAsync(int año);
    }

    /// <summary>
    /// Singleton que mantiene los feriados en memoria.
    /// Se cargan una sola vez al iniciar la app desde Program.cs.
    /// </summary>
    public class FeriadoService : IFeriadoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FeriadoService> _logger;

        // año → (fecha "yyyy-MM-dd" → nombre del feriado)
        private readonly Dictionary<int, Dictionary<string, string>> _cache = new();

        public FeriadoService(HttpClient httpClient, ILogger<FeriadoService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Devuelve los feriados de un año. Si no están cargados, retorna vacío.
        /// </summary>
        public Dictionary<string, string> GetFeriados(int año)
        {
            return _cache.TryGetValue(año, out var feriados)
                ? feriados
                : new Dictionary<string, string>();
        }

        /// <summary>
        /// Carga los feriados de un año desde la API. Se llama desde Program.cs al iniciar.
        /// </summary>
        public async Task CargarFeriadosAsync(int año)
        {
            if (_cache.ContainsKey(año)) return;

            try
            {
                var url = $"https://date.nager.at/api/v3/PublicHolidays/{año}/AR";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("No se pudieron cargar feriados para {Año}: {Status}", año, response.StatusCode);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var holidays = JsonSerializer.Deserialize<List<NagerHoliday>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var dict = new Dictionary<string, string>();
                foreach (var h in holidays ?? new())
                {
                    dict[h.Date] = h.LocalName ?? h.Name ?? "";
                }

                _cache[año] = dict;
                _logger.LogInformation("Feriados cargados para {Año}: {Count} feriados", año, dict.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar feriados para {Año}", año);
            }
        }
    }

    internal class NagerHoliday
    {
        public string Date { get; set; } = "";
        public string? LocalName { get; set; }
        public string? Name { get; set; }
    }
}