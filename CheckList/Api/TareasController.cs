using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Controllers
{
    [ApiController]
    [Route("api/tareas")]
    public class TareasController : ControllerBase
    {
        private readonly ITareaService _tareaService;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<TareasController> _logger;

        public TareasController(ITareaService tareaService, IHubContext<ChecklistHub> hubContext, ILogger<TareasController> logger)
        {
            _tareaService = tareaService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpPost("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            try
            {
                if (id <= 0) return BadRequest();

                await _tareaService.ToggleTareaAsync(id);

                var tarea = await _tareaService.GetTareaByIdAsync(id);
                if (tarea == null) return NotFound();

                await _hubContext.Clients.Group("checklist").SendAsync("TaskToggled", new
                {
                    id = tarea.Id,
                    completada = tarea.Completada
                });

                return Ok(new { id = tarea.Id, completada = tarea.Completada });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle tarea {Id}", id);
                return StatusCode(500);
            }
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || !ids.Any()) return BadRequest();

                await _tareaService.ReordenarTareasAsync(ids);
                await _hubContext.Clients.Group("checklist").SendAsync("TasksUpdated");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reordenar tareas");
                return StatusCode(500);
            }
        }
    }
}