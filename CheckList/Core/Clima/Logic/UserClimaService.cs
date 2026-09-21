using CheckList.Core.Clima.Domain;
using CheckList.Core.Parametro.Logic;

namespace CheckList.Core.Clima.Logic
{
    public interface IUserClimaService
    {
        Task<ClimaResult> ObtenerClimaAsync(CancellationToken cancellationToken = default);
    }

    public class UserClimaService : IUserClimaService
    {
        private readonly IParametroService _parametroService;
        private readonly IClimaService _climaService;

        public UserClimaService(IParametroService parametroService, IClimaService climaService)
        {
            _parametroService = parametroService;
            _climaService = climaService;
        }

        public async Task<ClimaResult> ObtenerClimaAsync(CancellationToken cancellationToken = default)
        {
            var ciudad = await _parametroService.ObtenerAsync("CLIMA_CIUDAD");

            if (string.IsNullOrWhiteSpace(ciudad))
            {
                throw new InvalidOperationException("No está configurado el parámetro CLIMA_CIUDAD.");
            }

            var provincia = await _parametroService.ObtenerAsync("CLIMA_PROVINCIA");

            return await _climaService.ObtenerClimaAsync(ciudad, provincia, cancellationToken);
        }
    }
}
