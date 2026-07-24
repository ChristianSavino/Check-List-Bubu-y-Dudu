namespace CheckList.Core.Tarea.Domain
{
    public static class TareaStringConverter
    {
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

        public static string GetTipoTareaLabel(TipoTarea tipo)
        {
            return tipo switch
            {
                TipoTarea.Daily    => "Diaria",
                TipoTarea.Specific => "Específica",
                TipoTarea.Weekly   => "Semanal",
                TipoTarea.Event    => "Evento",
                TipoTarea.Birthday => "Cumpleaños",
                _                  => "Desconocida"
            };
        }

        public static string GetDayOfWeekLabel(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Sunday    => "Domingo",
                DayOfWeek.Monday    => "Lunes",
                DayOfWeek.Tuesday   => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday  => "Jueves",
                DayOfWeek.Friday    => "Viernes",
                DayOfWeek.Saturday  => "Sábado",
                _                   => dia.ToString()
            };
        }

        public static string GetPersonaLabel(PersonaType persona)
        {
            return persona switch
            {
                PersonaType.Bubu => "Bubu",
                PersonaType.Dudu => "Dudu",
                PersonaType.None => "Sin asignar",
                _                => "Desconocida"
            };
        }
    }
}
