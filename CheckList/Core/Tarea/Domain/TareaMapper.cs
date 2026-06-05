namespace CheckList.Core.Tarea.Domain
{
    /// <summary>
    /// Consolidada DTO unificado para tareas
    /// </summary>
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
    }

    /// <summary>
    /// Centraliza toda la lógica de mapeo entre TareaEntity y DTOs
    /// </summary>
    public static class TareaMapper
    {
        /// <summary>
        /// Mapea TareaEntity a TareaDto para listados simples (Index)
        /// </summary>
        public static TareaDto MapToSimpleDto(TareaEntity entity, DateTime? referenceDate = null)
        {
            var hoy = referenceDate ?? DateTime.Now.Date;

            // Calcular días de atraso
            int diasAtraso = 0;
            if (entity.Fecha.HasValue && entity.Fecha.Value.Date < hoy && !entity.Completada)
                diasAtraso = (hoy - entity.Fecha.Value.Date).Days;

            return new TareaDto
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Label = entity.Nombre, // Para compatibilidad con Index
                Completada = entity.Completada,
                Hora = entity.Hora ?? "",
                Persona = entity.Persona ?? "",
                TipoTarea = TareaStringConverter.TipoTareaToString(entity.Tipo),
                TipoTareaLabel = TareaStringConverter.GetTipoTareaLabel(entity.Tipo),
                Fecha = entity.Fecha?.ToString(TareaConstants.DATE_FORMAT),
                DiaSemana = TareaStringConverter.DayOfWeekToString(entity.DiaSemana),
                DiasAtraso = diasAtraso
            };
        }

        /// <summary>
        /// Mapea TareaEntity a TareaDto para listados completos
        /// </summary>
        public static TareaDto MapToDetailDto(TareaEntity entity, DateTime? referenceDate = null)
        {
            return MapToSimpleDto(entity, referenceDate);
        }

        /// <summary>
        /// Convierte múltiples entidades a DTOs
        /// </summary>
        public static List<TareaDto> MapToList(IEnumerable<TareaEntity> entities, DateTime? referenceDate = null)
        {
            return entities
                .Where(t => !string.IsNullOrWhiteSpace(t.Nombre))
                .Select(t => MapToSimpleDto(t, referenceDate))
                .ToList();
        }

        /// <summary>
        /// Mapea datos de formulario a TareaEntity (para crear)
        /// </summary>
        public static TareaEntity MapFromFormToEntity(
            string nombre,
            string tipo,
            string fecha,
            string hora,
            string persona,
            string diaSemana)
        {
            return new TareaEntity
            {
                Nombre = nombre,
                Tipo = TareaStringConverter.StringToTipoTarea(tipo),
                Fecha = !string.IsNullOrWhiteSpace(fecha) ? DateTime.Parse(fecha) : null,
                Hora = hora ?? "",
                Persona = persona ?? "",
                DiaSemana = TareaStringConverter.StringToDayOfWeek(diaSemana),
                Completada = false,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            };
        }

        /// <summary>
        /// Actualiza una TareaEntity existente con datos de formulario
        /// </summary>
        public static void UpdateEntityFromForm(
            TareaEntity entity,
            string nombre,
            string tipo,
            string fecha,
            string hora,
            string persona,
            string diaSemana)
        {
            entity.Nombre = nombre;
            entity.Tipo = TareaStringConverter.StringToTipoTarea(tipo);
            entity.Fecha = !string.IsNullOrWhiteSpace(fecha) ? DateTime.Parse(fecha) : null;
            entity.Hora = hora ?? "";
            entity.Persona = persona ?? "";
            entity.DiaSemana = TareaStringConverter.StringToDayOfWeek(diaSemana);
            entity.FechaActualizacion = DateTime.Now;
        }

        /// <summary>
        /// Convierte TareaEntity a ViewModel para edición
        /// </summary>
        public static TareaFormViewModel MapToFormViewModel(TareaEntity entity)
        {
            return new TareaFormViewModel
            {
                Id = entity.Id,
                Nombre = entity.Nombre,
                Tipo = TareaStringConverter.TipoTareaToString(entity.Tipo),
                Fecha = entity.Fecha?.ToString(TareaConstants.DATE_FORMAT) ?? "",
                Hora = entity.Hora ?? "",
                Persona = entity.Persona ?? "",
                DiaSemana = TareaStringConverter.DayOfWeekToString(entity.DiaSemana),
                IsEditing = true
            };
        }
    }

    /// <summary>
    /// ViewModel para el formulario de agregar/editar tareas
    /// </summary>
    public class TareaFormViewModel
    {
        public int? Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = TareaConstants.TIPO_DAILY;
        public string Fecha { get; set; } = "";
        public string Hora { get; set; } = "";
        public string Persona { get; set; } = "";
        public string DiaSemana { get; set; } = "";
        public bool IsEditing { get; set; }
        public string PageTitle => IsEditing ? "Modificar Tarea" : "Agregar Tarea";
    }
}
