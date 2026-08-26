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
                var todasLasTareas = await _tareaService.GetTodasLasTareasAsync();

                // 1. TAREAS ESPECÍFICAS - Año exacto
                CargarTareasEspecificas(todasLasTareas, hoy, Año, MesInicio);

                // 2. EVENTOS - Rango de fechas
                CargarEventos(todasLasTareas, hoy, Año, MesInicio);

                // 3. CUMPLEAÑOS - Se repiten cada año (solo mes/día)
                CargarCumpleaños(todasLasTareas, hoy, Año);

                // Feriados
                await _feriadoService.CargarFeriadosAsync(Año);
                FeriadosPorDia = _feriadoService.GetFeriados(Año);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar calendario");
            }
        }

        private void CargarTareasEspecificas(List<TareaEntity> tareas, DateTime hoy, int año, int mesInicio)
        {
            var tareasEspecificas = tareas
                .Where(t => t.Tipo == TipoTarea.Specific
                         && t.Fecha.HasValue
                         && t.Fecha.Value.Year == año
                         && t.Fecha.Value.Month >= mesInicio)
                .ToList();

            foreach (var tarea in tareasEspecificas)
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
        }

        private void CargarEventos(List<TareaEntity> tareas, DateTime hoy, int año, int mesInicio)
        {
            var eventos = tareas
                .Where(t => t.Tipo == TipoTarea.Event
                         && t.Fecha.HasValue && t.FechaFin.HasValue
                         && ((t.Fecha.Value.Year == año) || (t.FechaFin.Value.Year == año)))
                .ToList();

            foreach (var ev in eventos)
            {
                var inicio = ev.Fecha!.Value.Date;
                var fin = ev.FechaFin!.Value.Date;
                var totalDias = (fin - inicio).Days + 1;

                for (var d = inicio; d <= fin; d = d.AddDays(1))
                {
                    if (d.Year != año || d.Month < mesInicio) continue;

                    var key = d.ToString("yyyy-MM-dd");
                    if (!EventosPorDia.ContainsKey(key))
                        EventosPorDia[key] = new List<EventoCalendario>();

                    var diaNum = (d - inicio).Days + 1;
                    EventosPorDia[key].Add(new EventoCalendario
                    {
                        Nombre = $"{ev.Nombre} ({diaNum}/{totalDias})",
                        Persona = ev.Persona ?? "",
                        Completada = false,
                        DiasRestantes = (d - hoy).Days
                    });
                }
            }
        }

        private void CargarCumpleaños(List<TareaEntity> tareas, DateTime hoy, int año)
        {
            var cumpleaños = tareas
                .Where(t => t.Tipo == TipoTarea.Birthday && t.Fecha.HasValue)
                .ToList();

            foreach (var cumple in cumpleaños)
            {
                var diaOriginal = cumple.Fecha!.Value;
                var mes = diaOriginal.Month;
                var dia = diaOriginal.Day;

                var fechaCumpleEsteAño = DateTime.Now;

                // Manejar 29 de febrero en años no bisiestos
                try
                {
                    fechaCumpleEsteAño = new DateTime(año, mes, dia);
                }
                catch (ArgumentOutOfRangeException)
                {
                    fechaCumpleEsteAño = new DateTime(año, mes, dia - 1);
                }
                catch (Exception) { continue; }

                if(fechaCumpleEsteAño < DateTime.Now)
                {
                    continue;
                }

                var key = fechaCumpleEsteAño.ToString("yyyy-MM-dd");
                if (!EventosPorDia.ContainsKey(key))
                    EventosPorDia[key] = new List<EventoCalendario>();

                EventosPorDia[key].Add(new EventoCalendario
                {
                    Nombre = $"🎂 {cumple.Nombre}",
                    Persona = "",
                    Completada = false,
                    DiasRestantes = (fechaCumpleEsteAño - hoy).Days
                });
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
