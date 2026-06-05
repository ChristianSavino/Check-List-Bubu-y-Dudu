using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using CoreTareaDto = CheckList.Core.Tarea.Domain.TareaDto;

namespace CheckList.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<IndexModel> _logger;

        public List<TareaDto> Daily { get; set; } = new();
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

                // Obtener todas las tareas
                var tareasDiarias = await _tareaService.GetTareasDiariaAsync();
                var tareasSemanalesHoy = await _tareaService.GetTareasSemanalesHoyAsync();
                var tareasSemanalesMañana = await _tareaService.GetTareasSemanalesMañanaAsync();
                var tareasHoy = await _tareaService.GetTareasHoyAsync();
                var tareasMañana = await _tareaService.GetTareasMañanaAsync();

                // Mapear tareas diarias
                Daily = ConvertirDtos(TareaMapper.MapToList(tareasDiarias, hoy));

                // Combinar tareas de hoy: específicas + semanales del día
                var tareasHoyCombinadas = new List<CoreTareaDto>();
                tareasHoyCombinadas.AddRange(TareaMapper.MapToList(tareasHoy, hoy));
                tareasHoyCombinadas.AddRange(TareaMapper.MapToList(tareasSemanalesHoy, hoy));
                Today = ConvertirDtos(tareasHoyCombinadas);

                // Combinar tareas de mañana: específicas + semanales del día
                var tareasMañanaCombinadas = new List<CoreTareaDto>();
                tareasMañanaCombinadas.AddRange(TareaMapper.MapToList(tareasMañana, hoy));
                tareasMañanaCombinadas.AddRange(TareaMapper.MapToList(tareasSemanalesMañana, hoy));
                Tomorrow = ConvertirDtos(tareasMañanaCombinadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tareas");
            }
        }

        /// <summary>
        /// Convierte CoreTareaDto a TareaDto para la UI incluyendo el tipo de tarea
        /// </summary>
        private static List<TareaDto> ConvertirDtos(List<CoreTareaDto> dtos)
        {
            return dtos.Select(d => new TareaDto
            {
                Id = d.Id,
                Label = d.Nombre,
                Done = d.Completada,
                Time = d.Hora,
                Persona = d.Persona,
                DiasAtraso = d.DiasAtraso,
                TipoTarea = d.TipoTarea,
                TipoTareaLabel = d.TipoTareaLabel
            }).ToList();
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
    }

    /// <summary>
    /// DTO para la página Index con propiedades específicas de la UI
    /// </summary>
    public class TareaDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = "";
        public bool Done { get; set; }
        public string Time { get; set; } = "";
        public string Persona { get; set; } = "";
        public int DiasAtraso { get; set; }
        public string TipoTarea { get; set; } = "";
        public string TipoTareaLabel { get; set; } = "";
    }
}