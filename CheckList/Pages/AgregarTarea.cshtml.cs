using CheckList.Core.Persona.DataAccess;
using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Pages
{
    public class AgregarTareaModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly IPersonaRepository _personaRepository;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<AgregarTareaModel> _logger;

        [BindProperty] public string Nombre { get; set; } = "";
        [BindProperty] public string Tipo { get; set; } = TareaConstants.TIPO_DAILY;
        [BindProperty] public string Fecha { get; set; } = "";
        [BindProperty] public string FechaFin { get; set; } = "";
        [BindProperty] public string Hora { get; set; } = "";
        [BindProperty] public string Persona { get; set; } = "";
        [BindProperty] public string DiaSemana { get; set; } = "";

        public int? TareaId { get; set; }
        public bool IsEditing { get; set; }
        public string PageTitle { get; set; } = "Agregar Tarea";

        // Personas dinámicas desde DB
        public List<string> PersonasDisponibles { get; set; } = new();

        public AgregarTareaModel(ITareaService tareaService, IPersonaRepository personaRepository, IHubContext<ChecklistHub> hubContext, ILogger<AgregarTareaModel> logger)
        {
            _tareaService = tareaService;
            _personaRepository = personaRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task OnGetAsync(int? id)
        {
            try
            {
                await CargarPersonas();

                TareaId = id;
                IsEditing = id.HasValue;

                if (IsEditing)
                {
                    PageTitle = "Modificar Tarea";
                    var tarea = await _tareaService.GetTareaByIdAsync(id.Value);
                    if (tarea != null)
                    {
                        var viewModel = TareaMapper.MapToFormViewModel(tarea);
                        Nombre    = viewModel.Nombre;
                        Tipo      = viewModel.Tipo;
                        Fecha     = viewModel.Fecha;
                        FechaFin  = viewModel.FechaFin;
                        Hora      = viewModel.Hora;
                        Persona   = viewModel.Persona;
                        DiaSemana = viewModel.DiaSemana;
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
                LimpiarCamposPorTipo();

                var errores = TareaValidator.ValidateFormInput(Nombre, Tipo, Fecha, DiaSemana, FechaFin);
                if (errores.Any())
                {
                    await CargarPersonas();
                    foreach (var error in errores)
                        ModelState.AddModelError(error.Field, error.Message);
                    return Page();
                }

                if (id.HasValue)
                {
                    var tarea = await _tareaService.GetTareaByIdAsync(id.Value);
                    if (tarea != null)
                    {
                        TareaMapper.UpdateEntityFromForm(tarea, Nombre, Tipo, Fecha, Hora, Persona, DiaSemana, FechaFin);
                        await _tareaService.ActualizarTareaAsync(tarea);
                    }
                }
                else
                {
                    var tarea = TareaMapper.MapFromFormToEntity(Nombre, Tipo, Fecha, Hora, Persona, DiaSemana, FechaFin);
                    await _tareaService.CrearTareaAsync(tarea);
                }

                await _hubContext.Clients.Group("checklist").SendAsync("TasksUpdated");
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar tarea");
                await CargarPersonas();
                ModelState.AddModelError("", "Error al guardar la tarea");
                return Page();
            }
        }

        private async Task CargarPersonas()
        {
            var personas = await _personaRepository.GetAllAsync();
            PersonasDisponibles = personas.Select(p => p.Nombre).ToList();
        }

        private void LimpiarCamposPorTipo()
        {
            var tipoTarea = TareaStringConverter.StringToTipoTarea(Tipo);

            switch (tipoTarea)
            {
                case TipoTarea.Daily:
                    Hora = ""; Fecha = ""; FechaFin = ""; DiaSemana = "";
                    ModelState.Remove("Hora"); ModelState.Remove("Fecha");
                    ModelState.Remove("FechaFin"); ModelState.Remove("DiaSemana");
                    break;
                case TipoTarea.Weekly:
                    Hora = ""; Fecha = ""; FechaFin = "";
                    ModelState.Remove("Hora"); ModelState.Remove("Fecha");
                    ModelState.Remove("FechaFin");
                    break;
                case TipoTarea.Specific:
                    DiaSemana = ""; FechaFin = "";
                    ModelState.Remove("DiaSemana"); ModelState.Remove("FechaFin");
                    break;
                case TipoTarea.Event:
                    DiaSemana = ""; Hora = "";
                    ModelState.Remove("DiaSemana"); ModelState.Remove("Hora");
                    break;
            }
        }
    }
}
