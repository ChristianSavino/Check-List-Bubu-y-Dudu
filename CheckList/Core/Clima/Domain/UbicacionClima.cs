namespace CheckList.Core.Clima.Domain
{
    public class UbicacionClima
    {
        public string Ciudad { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Timezone { get; set; } = string.Empty;
    }
}
