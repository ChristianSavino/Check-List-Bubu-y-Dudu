namespace CheckList.Core.Clima.Domain
{
    public class AlertaClima
    {
        public string Nivel { get; set; } = string.Empty;
        public string Fenomeno { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Inicio { get; set; }
        public DateTime? Fin { get; set; }
    }
}
