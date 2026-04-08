using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheckList.Core.Tarea.Logic;
using CheckList.Core.Tarea.Domain;

namespace CheckList.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<IndexModel> _logger;

        public List<TareaDto> Daily { get; set; } = new();
        public List<TareaDto> Today { get; set; } = new();
        public List<TareaDto> Tomorrow { get; set; } = new();

        public IndexModel(ITareaService tareaService, ILogger<IndexModel> logger)
        {
            _tareaService = tareaService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var tareasDiarias = await _tareaService.GetTareasDiariaAsync();
                var tareasHoy = await _tareaService.GetTareasHoyAsync();
                var tareasMañana = await _tareaService.GetTareasMañanaAsync();

                // Filtrar y mapear tareas diarias
                Daily = tareasDiarias
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => new TareaDto
                    {
                        Id = t.Id,
                        Label = t.Nombre,
                        Done = t.Completada,
                        Persona = t.Persona ?? ""
                    }).ToList();

                // Tareas específicas de hoy (ya filtradas en el repositorio)
                Today = tareasHoy
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => new TareaDto
                    {
                        Id = t.Id,
                        Label = t.Nombre,
                        Done = t.Completada,
                        Time = t.Hora ?? "",
                        Persona = t.Persona ?? ""
                    }).ToList();

                // Filtrar tareas de mañana
                Tomorrow = tareasMañana
                    .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                    .Select(t => new TareaDto
                    {
                        Id = t.Id,
                        Label = t.Nombre,
                        Done = t.Completada,
                        Time = t.Hora ?? "",
                        Persona = t.Persona ?? ""
                    }).ToList();
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
                {
                    return new BadRequestResult();
                }

                await _tareaService.ToggleTareaAsync(id);
                return new OkResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al toggle tarea");
                return new BadRequestResult();
            }
        }
    }

    public class TareaDto
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool Done { get; set; }
        public string Time { get; set; }
        public string Persona { get; set; }
    }
}