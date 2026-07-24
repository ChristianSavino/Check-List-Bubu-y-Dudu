namespace CheckList.Core.Tarea.Domain
{
    public static class TareaValidator
    {
        public class ValidationError
        {
            public string Field { get; set; }
            public string Message { get; set; }

            public ValidationError(string field, string message)
            {
                Field = field;
                Message = message;
            }
        }

        public static List<ValidationError> ValidateFormInput(
            string nombre, string tipo, string fecha,
            string diaSemana, string fechaFin = "")
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                errors.Add(new ValidationError("Nombre", "El nombre de la tarea es obligatorio"));
                return errors;
            }

            if (string.IsNullOrWhiteSpace(tipo))
            {
                errors.Add(new ValidationError("Tipo", "El tipo de tarea es obligatorio"));
                return errors;
            }

            var tipoTarea = TareaStringConverter.StringToTipoTarea(tipo);

            switch (tipoTarea)
            {
                case TipoTarea.Specific:
                    if (string.IsNullOrWhiteSpace(fecha))
                        errors.Add(new ValidationError("Fecha", "La fecha es obligatoria para tareas específicas"));
                    break;

                case TipoTarea.Weekly:
                    if (string.IsNullOrWhiteSpace(diaSemana))
                        errors.Add(new ValidationError("DiaSemana", "El día de la semana es obligatorio para tareas semanales"));
                    break;

                case TipoTarea.Event:
                    if (string.IsNullOrWhiteSpace(fecha))
                        errors.Add(new ValidationError("Fecha", "La fecha de inicio es obligatoria para eventos"));
                    if (string.IsNullOrWhiteSpace(fechaFin))
                        errors.Add(new ValidationError("FechaFin", "La fecha de fin es obligatoria para eventos"));

                    if (!string.IsNullOrWhiteSpace(fecha) && !string.IsNullOrWhiteSpace(fechaFin))
                    {
                        if (DateTime.TryParse(fecha, out var fi) && DateTime.TryParse(fechaFin, out var ff))
                        {
                            if (ff < fi)
                                errors.Add(new ValidationError("FechaFin", "La fecha de fin no puede ser anterior a la de inicio"));
                        }
                    }
                    break;

                case TipoTarea.Birthday:
                    if (string.IsNullOrWhiteSpace(fecha))
                        errors.Add(new ValidationError("Fecha", "La fecha del cumpleaños es obligatoria"));
                    break;
            }

            return errors;
        }

        public static List<ValidationError> ValidateEntity(TareaEntity tarea)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(tarea.Nombre))
                errors.Add(new ValidationError("Nombre", "El nombre de la tarea es requerido"));

            switch (tarea.Tipo)
            {
                case TipoTarea.Specific:
                    if (!tarea.Fecha.HasValue)
                        errors.Add(new ValidationError("Fecha", "Las tareas específicas requieren una fecha"));
                    break;
                case TipoTarea.Weekly:
                    if (!tarea.DiaSemana.HasValue)
                        errors.Add(new ValidationError("DiaSemana", "Las tareas semanales requieren un día de la semana"));
                    break;
                case TipoTarea.Event:
                    if (!tarea.Fecha.HasValue)
                        errors.Add(new ValidationError("Fecha", "Los eventos requieren fecha de inicio"));
                    if (!tarea.FechaFin.HasValue)
                        errors.Add(new ValidationError("FechaFin", "Los eventos requieren fecha de fin"));
                    if (tarea.Fecha.HasValue && tarea.FechaFin.HasValue && tarea.FechaFin < tarea.Fecha)
                        errors.Add(new ValidationError("FechaFin", "La fecha de fin no puede ser anterior a la de inicio"));
                    break;
                case TipoTarea.Birthday:
                    if (!tarea.Fecha.HasValue)
                        errors.Add(new ValidationError("Fecha", "Los cumpleaños requieren una fecha"));
                    break;
            }

            return errors;
        }

        public static ValidationError? GetFirstError(List<ValidationError> errors)
        {
            return errors.FirstOrDefault();
        }
    }
}
