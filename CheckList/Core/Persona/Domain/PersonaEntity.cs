namespace CheckList.Core.Persona.Domain
{
    public class PersonaEntity
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int Orden { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
