namespace CheckList.Core.Clima.Domain
{
    public class PronosticoDia
    {
        public DateTime Fecha { get; set; }
        public double TemperaturaMaxima { get; set; }
        public double TemperaturaMinima { get; set; }
        public double ProbabilidadLluvia { get; set; }
        public double Precipitacion { get; set; }
        public double VientoMaximo { get; set; }
        public string Icono { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
