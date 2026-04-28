using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class AgregarTareaModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly ILogger<AgregarTareaModel> _logger;

        [BindProperty]
        public string Nombre { get; set; }

        [BindProperty]
        public string Tipo { get; set; } = "daily";

        [BindProperty]
        public string Fecha { get; set; }

        [BindProperty]
        public string Hora { get; set; }

        [BindProperty]
        public string Persona { get; set; } = "";

        public int? TareaId { get; set; }
        public bool IsEditing { get; set; }
        public string PageTitle { get; set; } = "Agregar Tarea";

        public AgregarTareaModel(ITareaService tareaService, ILogger<AgregarTareaModel> logger)
        {
            _tareaService = tareaService;
            _logger = logger;
        }

        public async Task OnGetAsync(int? id)
        {
            try
            {
                TareaId = id;
                IsEditing = id.HasValue;

                if (IsEditing)
                {
                    PageTitle = "Modificar Tarea";
                    var tarea = await _tareaService.GetTareaByIdAsync(id.Value);
                    if (tarea != null)
                    {
                        Nombre = tarea.Nombre;
                        Tipo = tarea.Tipo.ToString().ToLower();
                        Fecha = tarea.Fecha?.ToString("yyyy-MM-dd") ?? "";
                        Hora = tarea.Hora ?? "";
                        Persona = tarea.Persona ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tarea");
            }
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            try
            {
                // Asegurar que Persona siempre tiene un valor válido
                if (Persona == null)
                {
                    Persona = "";
                }

                // CRÍTICO: Limpiar hora/fecha para tareas diarias
                if (Tipo == "daily")
                {
                    Hora = "";
                    Fecha = "";
                    ModelState.Remove("Hora");
                    ModelState.Remove("Fecha");
                }

                // VALIDACIÓN MANUAL - No depender de ModelState
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre de la tarea es obligatorio");
                    return Page();
                }

                // Validar que tarea específica tiene fecha
                if (Tipo == "specific" && string.IsNullOrWhiteSpace(Fecha))
                {
                    ModelState.AddModelError("Fecha", "La fecha es obligatoria para tareas específicas");
                    return Page();
                }

                if (id.HasValue)
                {
                    // Actualizar
                    var tarea = await _tareaService.GetTareaByIdAsync(id.Value);
                    if (tarea != null)
                    {
                        tarea.Nombre = Nombre;
                        tarea.Tipo = Enum.Parse<TipoTarea>(Tipo, true);
                        tarea.Fecha = !string.IsNullOrWhiteSpace(Fecha) ? DateTime.Parse(Fecha) : null;
                        tarea.Hora = Hora ?? "";
                        tarea.Persona = Persona ?? "";
                        await _tareaService.ActualizarTareaAsync(tarea);
                    }
                }
                else
                {
                    // Crear nueva
                    var tarea = new TareaEntity
                    {
                        Nombre = Nombre,
                        Tipo = Enum.Parse<TipoTarea>(Tipo, true),
                        Fecha = !string.IsNullOrWhiteSpace(Fecha) ? DateTime.Parse(Fecha) : null,
                        Hora = Hora ?? "",
                        Persona = Persona ?? "",
                        Completada = false
                    };
                    await _tareaService.CrearTareaAsync(tarea);
                }

                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar tarea");
                ModelState.AddModelError("", "Error al guardar la tarea");
                return Page();
            }
        }
    }
}
