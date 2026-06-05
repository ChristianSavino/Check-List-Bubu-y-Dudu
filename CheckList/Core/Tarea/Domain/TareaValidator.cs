namespace CheckList.Core.Tarea.Domain
{
    /// <summary>
    /// Centraliza todas las validaciones de tareas
    /// </summary>
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

        /// <summary>
        /// Valida que los campos del formulario sean correctos según el tipo de tarea
        /// </summary>
        public static List<ValidationError> ValidateFormInput(
            string nombre,
            string tipo,
            string fecha,
            string diaSemana)
        {
            var errors = new List<ValidationError>();

            // Validación de nombre (siempre requerido)
            if (string.IsNullOrWhiteSpace(nombre))
            {
                errors.Add(new ValidationError("Nombre", "El nombre de la tarea es obligatorio"));
                return errors; // No validar más si falta el nombre
            }

            // Convertir el tipo
            if (string.IsNullOrWhiteSpace(tipo))
            {
                errors.Add(new ValidationError("Tipo", "El tipo de tarea es obligatorio"));
                return errors;
            }

            var tipoTarea = TareaStringConverter.StringToTipoTarea(tipo);

            // Validaciones específicas por tipo
            switch (tipoTarea)
            {
                case TipoTarea.Specific:
                    if (string.IsNullOrWhiteSpace(fecha))
                    {
                        errors.Add(new ValidationError("Fecha", "La fecha es obligatoria para tareas específicas"));
                    }
                    break;

                case TipoTarea.Weekly:
                    if (string.IsNullOrWhiteSpace(diaSemana))
                    {
                        errors.Add(new ValidationError("DiaSemana", "El día de la semana es obligatorio para tareas semanales"));
                    }
                    break;
            }

            return errors;
        }

        /// <summary>
        /// Valida que una entidad de tarea tenga los datos requeridos
        /// </summary>
        public static List<ValidationError> ValidateEntity(TareaEntity tarea)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(tarea.Nombre))
            {
                errors.Add(new ValidationError("Nombre", "El nombre de la tarea es requerido"));
            }

            switch (tarea.Tipo)
            {
                case TipoTarea.Specific:
                    if (!tarea.Fecha.HasValue)
                    {
                        errors.Add(new ValidationError("Fecha", "Las tareas específicas requieren una fecha"));
                    }
                    break;

                case TipoTarea.Weekly:
                    if (!tarea.DiaSemana.HasValue)
                    {
                        errors.Add(new ValidationError("DiaSemana", "Las tareas semanales requieren un día de la semana"));
                    }
                    break;
            }

            return errors;
        }

        /// <summary>
        /// Obtiene el primer error o null si no hay errores
        /// </summary>
        public static ValidationError? GetFirstError(List<ValidationError> errors)
        {
            return errors.FirstOrDefault();
        }
    }
}
