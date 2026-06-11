namespace CheckList.Core.Tarea.Domain
{
    public class TareaEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoTarea Tipo { get; set; }
        public DateTime? Fecha { get; set; }       // NULL para Daily. Para Weekly es la última fecha asignada
        public DateTime? FechaFin { get; set; }    // Solo para Event (fecha fin del período)
        public DayOfWeek? DiaSemana { get; set; }  // Solo para Weekly
        public string Hora { get; set; }
        public string Persona { get; set; }        // "Bubu", "Dudu", o vacío
        public bool Completada { get; set; }
        public int Orden { get; set; }             // Para ordenamiento manual
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
