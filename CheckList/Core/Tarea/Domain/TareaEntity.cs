namespace CheckList.Core.Tarea.Domain
{
    public class TareaEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "daily" o "specific"
        public DateTime? Fecha { get; set; } // NULL para tareas diarias
        public string Hora { get; set; } // Opcional
        public string Persona { get; set; } // "Bubu", "Dudu", o vacío
        public bool Completada { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
