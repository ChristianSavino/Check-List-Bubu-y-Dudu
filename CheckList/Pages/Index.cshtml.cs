using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<IndexModel> _logger;

        public List<TareaDto> Daily { get; set; } = new();
        public List<TareaDto> Weekly { get; set; } = new();
        public List<TareaDto> Today { get; set; } = new();
        public List<TareaDto> Tomorrow { get; set; } = new();

        public IndexModel(ITareaService tareaService, IHubContext<ChecklistHub> hubContext, ILogger<IndexModel> logger)
        {
            _tareaService = tareaService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var hoy = DateTime.Now.Date;

                var tareasDiarias = await _tareaService.GetTareasDiariaAsync();
                var tareasSemanales = await _tareaService.GetTareasSemanalesHoyAsync();
                var tareasHoy = await _tareaService.GetTareasHoyAsync();
                var tareasMañana = await _tareaService.GetTareasMañanaAsync();

                Daily = tareasDiarias
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => MapToDto(t, hoy))
                    .ToList();

                Weekly = tareasSemanales
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => MapToDto(t, hoy))
                    .ToList();

                Today = tareasHoy
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => MapToDto(t, hoy))
                    .ToList();

                Tomorrow = tareasMañana
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => MapToDto(t, hoy))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tareas");
            }
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest();

                await _tareaService.ToggleTareaAsync(id);

                var tarea = await _tareaService.GetTareaByIdAsync(id);
                if (tarea != null)
                {
                    // Notificar a todos los clientes conectados via SignalR
                    await _hubContext.Clients.Group("checklist").SendAsync("TaskToggled", new
                    {
                        id = tarea.Id,
                        completada = tarea.Completada
                    });
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle tarea {Id}", id);
                return BadRequest();
            }
        }

        private static TareaDto MapToDto(TareaEntity t, DateTime hoy)
        {
            // Calcular días de atraso on-the-fly desde la Fecha original
            int diasAtraso = 0;
            if (t.Fecha.HasValue && t.Fecha.Value.Date < hoy && !t.Completada)
                diasAtraso = (hoy - t.Fecha.Value.Date).Days;

            return new TareaDto
            {
                Id = t.Id,
                Label = t.Nombre,
                Done = t.Completada,
                Time = t.Hora ?? "",
                Persona = t.Persona ?? "",
                DiasAtraso = diasAtraso
            };
        }
    }

    public class TareaDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public bool Done { get; set; }
        public string Time { get; set; } = "";
        public string Persona { get; set; } = "";
        public int DiasAtraso { get; set; }
    }
}