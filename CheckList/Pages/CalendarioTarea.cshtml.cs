using CheckList.Core.Tarea.Domain;
using CheckList.Core.Tarea.Logic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class CalendarioTareaModel : PageModel
    {
        private readonly ITareaService _tareaService;
        private readonly IFeriadoService _feriadoService;
        private readonly ILogger<CalendarioTareaModel> _logger;

        public int Año { get; set; }
        public int MesInicio { get; set; }
        public int AñoActual { get; set; }
        public Dictionary<string, List<EventoCalendario>> EventosPorDia { get; set; } = new();
        public Dictionary<string, string> FeriadosPorDia { get; set; } = new();

        public CalendarioTareaModel(ITareaService tareaService, IFeriadoService feriadoService, ILogger<CalendarioTareaModel> logger)
        {
            _tareaService = tareaService;
            _feriadoService = feriadoService;
            _logger = logger;
        }

        public async Task OnGetAsync(int? año)
        {
            try
            {
                AñoActual = DateTime.Now.Year;
                Año = año.HasValue && año.Value >= AñoActual ? año.Value : AñoActual;
                MesInicio = Año == AñoActual ? DateTime.Now.Month : 1;

                var hoy = DateTime.Now.Date;

                // Cargar tareas específicas
                var todasLasTareas = await _tareaService.GetTodasLasTareasAsync();
                var tareasDelAño = todasLasTareas
                    .Where(t => t.Tipo == TipoTarea.Specific
                             && t.Fecha.HasValue
                             && t.Fecha.Value.Year == Año
                             && t.Fecha.Value.Month >= MesInicio)
                    .ToList();

                foreach (var tarea in tareasDelAño)
                {
                    var key = tarea.Fecha!.Value.ToString("yyyy-MM-dd");
                    if (!EventosPorDia.ContainsKey(key))
                        EventosPorDia[key] = new List<EventoCalendario>();

                    EventosPorDia[key].Add(new EventoCalendario
                    {
                        Nombre = tarea.Nombre,
                        Hora = tarea.Hora ?? "",
                        Persona = tarea.Persona ?? "",
                        Completada = tarea.Completada,
                        DiasRestantes = (tarea.Fecha.Value.Date - hoy).Days
                    });
                }

                // Feriados: si el año no está en memoria, cargarlo (ej: navegaron a año futuro)
                await _feriadoService.CargarFeriadosAsync(Año);
                FeriadosPorDia = _feriadoService.GetFeriados(Año);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar calendario");
            }
        }
    }

    public class EventoCalendario
    {
        public string Nombre { get; set; } = "";
        public string Hora { get; set; } = "";
        public string Persona { get; set; } = "";
        public bool Completada { get; set; }
        public int DiasRestantes { get; set; }
    }
}
