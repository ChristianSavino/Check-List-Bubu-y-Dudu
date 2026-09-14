using System.Text.Json;
using CheckList.Core.Dolares.Domain;

namespace CheckList.Core.Dolares.Logic
{
    public interface IDolaresService
    {
        public Task<Cotizacion> ObtenerCotizaciones();
    }

    public class DolaresService : IDolaresService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DolaresService> _logger;
        private Cotizacion _ultimaCotizacion;

        public DolaresService(HttpClient httpClient, ILogger<DolaresService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Cotizacion> ObtenerCotizaciones()
        {
            try
            {
                if (_ultimaCotizacion != null && (DateTime.Now - _ultimaCotizacion.FechaActualizacion).TotalMinutes < 10)
                {
                    return _ultimaCotizacion;
                }

                var response = await _httpClient.GetAsync("https://dolarapi.com/v1/dolares");
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var cotizaciones = JsonSerializer.Deserialize<List<DolarCotizacion>>(responseJson, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

                    var blue = cotizaciones.First(c => c.Nombre == "Blue");
                    var oficial = cotizaciones.First(c => c.Nombre == "Oficial");

                    _ultimaCotizacion = new Cotizacion
                    {
                        BlueCompra = blue.Compra,
                        BlueVenta = blue.Venta,
                        OficialCompra = oficial.Compra,
                        OficialVenta = oficial.Venta,
                        FechaActualizacion = DateTime.Now
                    };

                    return _ultimaCotizacion;
                }
                else
                {
                    throw new Exception($"Error al obtener cotizaciones - {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cotizaciones");
                throw;
            }

        }
    }
}
