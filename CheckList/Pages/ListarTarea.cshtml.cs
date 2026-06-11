using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Pages
{
    public class ListarTareaModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<ListarTareaModel> _logger;

        public List<TareaListDto> Tareas { get; set; } = new();

        public ListarTareaModel(ITareaService tareaService, IHubContext<ChecklistHub> hubContext, ILogger<ListarTareaModel> logger)
        {
            _tareaService = tareaService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var tareas = await _tareaService.GetTodasLasTareasAsync();
                Tareas = tareas
                    .Select(t => new TareaListDto
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Tipo = t.Tipo.ToString(),
                        TipoLabel = TareaStringConverter.GetTipoTareaLabel(t.Tipo),
                        Fecha = t.Fecha?.ToString(TareaConstants.DATE_FORMAT),
                        FechaFin = t.FechaFin?.ToString(TareaConstants.DATE_FORMAT),
                        Hora = t.Hora ?? "",
                        Persona = t.Persona ?? "",
                        DiaSemana = t.DiaSemana.HasValue
                            ? TareaStringConverter.GetDayOfWeekLabel(t.DiaSemana.Value)
                            : "",
                        Orden = t.Orden
                    })
                    .OrderBy(t => t.Tipo == "Specific" ? 1 : 0)
                    .ThenBy(t =>
                        t.Tipo == "Specific" && !string.IsNullOrEmpty(t.Fecha)
                            ? DateTime.Parse(t.Fecha)
                            : DateTime.MinValue)
                    .ThenBy(t =>
                        t.Tipo == "Specific"
                            ? t.Hora
                            : "")
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tareas");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _tareaService.EliminarTareaAsync(id);

                // Notificar a todos los clientes
                await _hubContext.Clients.Group("checklist").SendAsync("TasksUpdated");

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tarea");
                return RedirectToPage();
            }
        }
    }

    public class TareaListDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string TipoLabel { get; set; } = "";
        public string? Fecha { get; set; }
        public string? FechaFin { get; set; }
        public string Hora { get; set; } = "";
        public string Persona { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public int Orden { get; set; }
    }
}
