using Microsoft.AspNetCore.Mvc;
using CheckList.Core.Tarea.Logic;

namespace CheckList.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TareasController : ControllerBase
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<TareasController> _logger;

        public TareasController(ITareaService tareaService, ILogger<TareasController> logger)
        {
            _tareaService = tareaService;
            _logger = logger;
        }

        [HttpPost("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID debe ser mayor que 0");
                }

                await _tareaService.ToggleTareaAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle tarea");
                return StatusCode(500, "Error al toggle tarea");
            }
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return BadRequest("IDs no pueden estar vacías");
                }

                await _tareaService.ReordenarTareasAsync(ids);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reordenar tareas");
                return StatusCode(500, "Error al reordenar tareas");
            }
        }
    }
}
