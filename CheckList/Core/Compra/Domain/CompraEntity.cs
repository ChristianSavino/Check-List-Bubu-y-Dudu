namespace CheckList.Core.Compra.Domain
{
    public enum TipoCompra
    {
        Diaria,
        Otra
    }

    public class CompraEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public TipoCompra Tipo { get; set; }
        public bool Completada { get; set; }
        public int Orden { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
