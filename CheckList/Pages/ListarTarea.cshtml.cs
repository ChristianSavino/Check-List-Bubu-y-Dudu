using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheckList.Core.Tarea.Logic;
using CheckList.Core.Tarea.Domain;

namespace CheckList.Pages
{
    public class ListarTareaModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<ListarTareaModel> _logger;

        public List<TareaListDto> Tareas { get; set; } = new();

        public ListarTareaModel(ITareaService tareaService, ILogger<ListarTareaModel> logger)
        {
            _tareaService = tareaService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var tareas = await _tareaService.GetTodasLasTareasAsync();
                Tareas = tareas.Select(t => new TareaListDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Tipo = t.Tipo,
                    TipoLabel = t.Tipo == "daily" ? "Diaria" : "Específica",
                    Fecha = t.Fecha?.ToString("yyyy-MM-dd"),
                    Hora = t.Hora ?? "",
                    Persona = t.Persona ?? ""
                }).ToList();
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
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string TipoLabel { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Persona { get; set; }
    }
}
