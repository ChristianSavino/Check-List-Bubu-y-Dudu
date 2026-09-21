namespace CheckList.Core.Clima.Domain
{
    public class ClimaResult
    {
        public UbicacionClima Ubicacion { get; set; } = new();
        public ClimaActual Actual { get; set; } = new();
        public List<PronosticoHora> Horas { get; set; } = [];
        public List<PronosticoDia> Dias { get; set; } = [];
        public List<AlertaClima> Alertas { get; set; } = [];
        public DateTime Actualizado { get; set; }
    }
}
