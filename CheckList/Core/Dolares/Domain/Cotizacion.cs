namespace CheckList.Core.Dolares.Domain
{
    public class Cotizacion
    {
        public decimal OficialVenta { get; set; }
        public decimal OficialCompra { get; set; }
        public decimal BlueVenta { get; set; }
        public decimal BlueCompra { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public class DolarCotizacion
    {
        public string Casa { get; set; }
        public string Nombre { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
