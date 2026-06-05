namespace CheckList.Core.Tarea.Domain
{
    /// <summary>
    /// Centraliza todas las conversiones entre strings (UI) y enums
    /// </summary>
    public static class TareaStringConverter
    {
        // Conversiones TipoTarea <-> String
        public static TipoTarea StringToTipoTarea(string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return TipoTarea.Daily;

            return Enum.Parse<TipoTarea>(tipo, ignoreCase: true);
        }

        public static string TipoTareaToString(TipoTarea tipo)
        {
            return tipo.ToString().ToLower();
        }

        // Conversiones DayOfWeek <-> String
        public static DayOfWeek? StringToDayOfWeek(string dia)
        {
            if (string.IsNullOrWhiteSpace(dia))
                return null;

            return Enum.Parse<DayOfWeek>(dia, ignoreCase: true);
        }

        public static string DayOfWeekToString(DayOfWeek? dia)
        {
            return dia?.ToString() ?? "";
        }

        // Conversiones PersonaType <-> String
        public static PersonaType StringToPersonaType(string persona)
        {
            if (string.IsNullOrWhiteSpace(persona))
                return PersonaType.None;

            return Enum.Parse<PersonaType>(persona, ignoreCase: true);
        }

        public static string PersonaTypeToString(PersonaType persona)
        {
            return persona == PersonaType.None ? "" : persona.ToString();
        }

        // Etiquetas para UI
        public static string GetTipoTareaLabel(TipoTarea tipo)
        {
            return tipo switch
            {
                TipoTarea.Daily => "Diaria",
                TipoTarea.Specific => "Específica",
                TipoTarea.Weekly => "Semanal",
                _ => "Desconocida"
            };
        }

        public static string GetDayOfWeekLabel(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                _ => dia.ToString()
            };
        }

        public static string GetPersonaLabel(PersonaType persona)
        {
            return persona switch
            {
                PersonaType.Bubu => "Bubu",
                PersonaType.Dudu => "Dudu",
                PersonaType.None => "Sin asignar",
                _ => "Desconocida"
            };
        }
    }
}
