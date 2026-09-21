using CheckList.Core.Parametro.Logic;
using Microsoft.AspNetCore.Mvc;

namespace CheckList.Api
{
    [ApiController]
    [Route("api/parametros")]
    public class ParametrosController : ControllerBase
    {
        private readonly IParametroService _parametroService;

        public ParametrosController(IParametroService parametroService)
        {
            _parametroService = parametroService;
        }

        [HttpPost("clima")]
        public async Task<IActionResult> GuardarClima(
            [FromBody] ClimaParametroDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ciudad))
                return BadRequest("La ciudad es obligatoria.");

            await _parametroService.GuardarAsync(
                "CLIMA_CIUDAD",
                dto.Ciudad.Trim());

            await _parametroService.GuardarAsync(
                "CLIMA_PROVINCIA",
                dto.Provincia?.Trim() ?? "");

            return Ok(new
            {
                ciudad = dto.Ciudad.Trim(),
                provincia = dto.Provincia?.Trim() ?? ""
            });
        }
    }

    public class ClimaParametroDto
    {
        public string Ciudad { get; set; } = "";
        public string Provincia { get; set; } = "";
    }
}
