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
                var mañana = hoy.AddDays(1);

                var tareasDiarias         = await _tareaService.GetTareasDiariaAsync();
                var tareasSemanalesHoy    = await _tareaService.GetTareasSemanalesHoyAsync();
                var tareasSemanalesMañana = await _tareaService.GetTareasSemanalesMañanaAsync();
                var tareasHoy             = await _tareaService.GetTareasHoyAsync();
                var tareasMañana          = await _tareaService.GetTareasMañanaAsync();
                var eventosHoy            = await _tareaService.GetEventosActivosEnFechaAsync(hoy);
                var eventosMañana         = await _tareaService.GetEventosActivosEnFechaAsync(mañana);

                Daily = ConvertirDtos(TareaMapper.MapToList(tareasDiarias, hoy));

                // Hoy: específicas + semanales + eventos del día
                var tareasHoyCombinadas = new List<CoreTareaDto>();
                tareasHoyCombinadas.AddRange(TareaMapper.MapToList(tareasHoy, hoy));
                tareasHoyCombinadas.AddRange(TareaMapper.MapToList(tareasSemanalesHoy, hoy));
                tareasHoyCombinadas.AddRange(TareaMapper.MapEventosParaDia(eventosHoy, hoy));
                Today = ConvertirDtos(tareasHoyCombinadas);

                // Mañana: específicas + semanales + eventos
                var tareasMañanaCombinadas = new List<CoreTareaDto>();
                tareasMañanaCombinadas.AddRange(TareaMapper.MapToList(tareasMañana, hoy));
                tareasMañanaCombinadas.AddRange(TareaMapper.MapToList(tareasSemanalesMañana, hoy));
                tareasMañanaCombinadas.AddRange(TareaMapper.MapEventosParaDia(eventosMañana, mañana));
                Tomorrow = ConvertirDtos(tareasMañanaCombinadas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tareas");
            }
        }

        private static List<TareaDto> ConvertirDtos(List<CoreTareaDto> dtos)
        {
            return dtos.Select(d => new TareaDto
            {
                Id = d.Id,
                Label = d.Label, // Para eventos ya incluye "Nombre (3/10)"
                Done = d.Completada,
                Time = d.Hora,
                Persona = d.Persona,
                DiasAtraso = d.DiasAtraso,
                TipoTarea = d.TipoTarea,
                TipoTareaLabel = d.TipoTareaLabel,
                EsEvento = d.EsEvento,
                EventoProgreso = d.EventoProgreso
            }).ToList();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            try
            {
                if (id <= 0) return BadRequest();

                // No permitir toggle en eventos
                var tarea = await _tareaService.GetTareaByIdAsync(id);
                if (tarea == null) return NotFound();
                if (tarea.Tipo == TipoTarea.Event) return BadRequest();

                await _tareaService.ToggleTareaAsync(id);

                tarea = await _tareaService.GetTareaByIdAsync(id);
                if (tarea != null)
                {
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
        public bool EsEvento { get; set; }
        public string EventoProgreso { get; set; } = "";
    }
}
