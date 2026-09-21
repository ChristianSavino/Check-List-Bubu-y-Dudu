namespace CheckList.Core.Clima.Domain
{
    public class PronosticoHora
    {
        public DateTime Hora { get; set; }
        public double Temperatura { get; set; }
        public double SensacionTermica { get; set; }
        public int Humedad { get; set; }
        public double Viento { get; set; }
        public double Rafaga { get; set; }
        public int ProbabilidadLluvia { get; set; }
        public double Precipitacion { get; set; }
        public int CodigoWmo { get; set; }
        public string Icono { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
