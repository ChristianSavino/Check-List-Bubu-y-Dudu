namespace CheckList.Core.Tarea.Domain
{
    public class TareaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Label { get; set; } = "";
        public bool Completada { get; set; }
        public string Hora { get; set; } = "";
        public string Persona { get; set; } = "";
        public string TipoTarea { get; set; } = "";
        public string TipoTareaLabel { get; set; } = "";
        public string? Fecha { get; set; }
        public string? DiaSemana { get; set; }
        public int DiasAtraso { get; set; }
        public bool EsEvento { get; set; }
        public string EventoProgreso { get; set; } = ""; // "3/10"
    }

    public static class TareaMapper
    {
        public static TareaDto MapToSimpleDto(TareaEntity entity, DateTime? referenceDate = null)
        {
            var hoy = referenceDate ?? DateTime.Now.Date;

            int diasAtraso = 0;
            if (entity.Tipo != TipoTarea.Event && entity.Tipo != TipoTarea.Birthday && entity.Fecha.HasValue
                && entity.Fecha.Value.Date < hoy && !entity.Completada)
                diasAtraso = (hoy - entity.Fecha.Value.Date).Days;

            return new TareaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Label = entity.Nombre,
                Completada = entity.Completada,
                Hora = entity.Hora ?? "",
                Persona = entity.Persona ?? "",
                TipoTarea = TareaStringConverter.TipoTareaToString(entity.Tipo),
                TipoTareaLabel = TareaStringConverter.GetTipoTareaLabel(entity.Tipo),
                Fecha = entity.Fecha?.ToString(TareaConstants.DATE_FORMAT),
                DiaSemana = TareaStringConverter.DayOfWeekToString(entity.DiaSemana),
                DiasAtraso = diasAtraso,
                EsEvento = entity.Tipo == TipoTarea.Event || entity.Tipo == TipoTarea.Birthday
            };
        }

        public static TareaDto MapToDetailDto(TareaEntity entity, DateTime? referenceDate = null)
        {
            return MapToSimpleDto(entity, referenceDate);
        }

        public static List<TareaDto> MapToList(IEnumerable<TareaEntity> entities, DateTime? referenceDate = null)
        {
            return entities
                .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                .Select(t => MapToSimpleDto(t, referenceDate))
                .ToList();
        }

        /// <summary>
        /// Genera DTOs de eventos activos para un día específico.
        /// Un evento de 10 días genera "Vacaciones (3/10)" para el día 3.
        /// Para cumpleaños, solo muestra el nombre sin contador.
        /// </summary>
        public static List<TareaDto> MapEventosParaDia(IEnumerable<TareaEntity> eventos, DateTime dia)
        {
            var result = new List<TareaDto>();

            foreach (var ev in eventos)
            {
                if ((ev.Tipo != TipoTarea.Event && ev.Tipo != TipoTarea.Birthday) || !ev.Fecha.HasValue)
                    continue;

                // Para cumpleaños, comparar solo mes y día
                if (ev.Tipo == TipoTarea.Birthday)
                {
                    var diaDelCumpleaños = ev.Fecha.Value.Date;
                    if (diaDelCumpleaños.Month == dia.Date.Month && diaDelCumpleaños.Day == dia.Date.Day)
                    {
                        result.Add(new TareaDto
                        {
                            Id = ev.Id,
                            Nombre = ev.Nombre,
                            Label = ev.Nombre,
                            Completada = false,
                            Hora = "",
                            Persona = "", // Sin persona para cumpleaños
                            TipoTarea = TareaStringConverter.TipoTareaToString(ev.Tipo),
                            TipoTareaLabel = TareaStringConverter.GetTipoTareaLabel(ev.Tipo),
                            Fecha = ev.Fecha?.ToString(TareaConstants.DATE_FORMAT),
                            EsEvento = true,
                            EventoProgreso = ""
                        });
                    }
                    continue;
                }

                // Para eventos normales, verificar rango de fechas
                if (!ev.FechaFin.HasValue)
                    continue;

                var inicio = ev.Fecha.Value.Date;
                var fin = ev.FechaFin.Value.Date;

                if (dia.Date < inicio || dia.Date > fin)
                    continue;

                var diaActual = (dia.Date - inicio).Days + 1;
                var totalDias = (fin - inicio).Days + 1;

                result.Add(new TareaDto
                {
                    Id = ev.Id,
                    Nombre = ev.Nombre,
                    Label = $"{ev.Nombre} ({diaActual}/{totalDias})",
                    Completada = false, // Events never complete
                    Hora = "",
                    Persona = ev.Persona ?? "",
                    TipoTarea = TareaStringConverter.TipoTareaToString(ev.Tipo),
                    TipoTareaLabel = TareaStringConverter.GetTipoTareaLabel(ev.Tipo),
                    Fecha = ev.Fecha?.ToString(TareaConstants.DATE_FORMAT),
                    EsEvento = true,
                    EventoProgreso = $"{diaActual}/{totalDias}"
                });
            }

            return result;
        }

        // ─── Form ↔ Entity ───────────────────────────────────────────────

        public static TareaEntity MapFromFormToEntity(
            string nombre, string tipo, string fecha, string hora,
            string persona, string diaSemana, string fechaFin = "")
        {
            return new TareaEntity
            {
                Nombre = nombre,
                Tipo = TareaStringConverter.StringToTipoTarea(tipo),
                Fecha = !string.IsNullOrWhiteSpace(fecha) ? DateTime.Parse(fecha) : null,
                FechaFin = !string.IsNullOrWhiteSpace(fechaFin) ? DateTime.Parse(fechaFin) : null,
                Hora = hora ?? "",
                Persona = persona ?? "",
                DiaSemana = TareaStringConverter.StringToDayOfWeek(diaSemana),
                Completada = false,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }

        public static void UpdateEntityFromForm(
            TareaEntity entity, string nombre, string tipo, string fecha,
            string hora, string persona, string diaSemana, string fechaFin = "")
        {
            entity.Nombre = nombre;
            entity.Tipo = TareaStringConverter.StringToTipoTarea(tipo);
            entity.Fecha = !string.IsNullOrWhiteSpace(fecha) ? DateTime.Parse(fecha) : null;
            entity.FechaFin = !string.IsNullOrWhiteSpace(fechaFin) ? DateTime.Parse(fechaFin) : null;
            entity.Hora = hora ?? "";
            entity.Persona = persona ?? "";
            entity.DiaSemana = TareaStringConverter.StringToDayOfWeek(diaSemana);
            entity.FechaActualizacion = DateTime.Now;
        }

        public static TareaFormViewModel MapToFormViewModel(TareaEntity entity)
        {
            return new TareaFormViewModel
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Tipo = TareaStringConverter.TipoTareaToString(entity.Tipo),
                Fecha = entity.Fecha?.ToString(TareaConstants.DATE_FORMAT) ?? "",
                FechaFin = entity.FechaFin?.ToString(TareaConstants.DATE_FORMAT) ?? "",
                Hora = entity.Hora ?? "",
                Persona = entity.Persona ?? "",
                DiaSemana = TareaStringConverter.DayOfWeekToString(entity.DiaSemana),
                IsEditing = true
            };
        }
    }

    public class TareaFormViewModel
    {
        public int? Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = TareaConstants.TIPO_DAILY;
        public string Fecha { get; set; } = "";
        public string FechaFin { get; set; } = "";
        public string Hora { get; set; } = "";
        public string Persona { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool IsEditing { get; set; }
        public string PageTitle => IsEditing ? "Modificar Tarea" : "Agregar Tarea";
    }
}
