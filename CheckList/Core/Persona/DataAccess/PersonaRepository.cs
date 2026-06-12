using CheckList.Core.Persona.Domain;
using CheckList.Core.Tarea.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace CheckList.Core.Persona.DataAccess
{
    public interface IPersonaRepository
    {
        Task<List<PersonaEntity>> GetAllAsync();
        Task<PersonaEntity?> GetByIdAsync(int id);
        Task<PersonaEntity?> GetByNombreAsync(string nombre);
        Task AddAsync(PersonaEntity persona);
        Task UpdateAsync(PersonaEntity persona);
        Task DeleteAsync(int id);
        Task LimpiarPersonaEnTareasAsync(string nombre);
        Task RenombrarPersonaEnTareasAsync(string nombreViejo, string nombreNuevo);
        Task RenombrarPersonaEnComprasAsync(string nombreViejo, string nombreNuevo);
        Task SeedDefaultsAsync();
    }

    public class PersonaRepository : IPersonaRepository
    {
        private readonly CheckListDbContext _context;

        public PersonaRepository(CheckListDbContext context)
        {
            _context = context;
        }

        public async Task<List<PersonaEntity>> GetAllAsync()
        {
            return await _context.Personas
                .OrderBy(p => p.Orden)
                .ToListAsync();
        }

        public async Task<PersonaEntity?> GetByIdAsync(int id)
        {
            return await _context.Personas.FindAsync(id);
        }

        public async Task<PersonaEntity?> GetByNombreAsync(string nombre)
        {
            return await _context.Personas
                .FirstOrDefaultAsync(p => p.Nombre == nombre);
        }

        public async Task AddAsync(PersonaEntity persona)
        {
            var maxOrden = await _context.Personas.MaxAsync(p => (int?)p.Orden) ?? 0;
            persona.Orden = maxOrden + 1;
            persona.FechaCreacion = DateTime.Now;
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PersonaEntity persona)
        {
            _context.Personas.Update(persona);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona != null)
            {
                // Limpiar tareas asignadas a esta persona
                await LimpiarPersonaEnTareasAsync(persona.Nombre);

                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
            }
        }

        public async Task LimpiarPersonaEnTareasAsync(string nombre)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Tareas SET Persona = '' WHERE Persona = {0}", nombre);
        }

        public async Task RenombrarPersonaEnTareasAsync(string nombreViejo, string nombreNuevo)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Tareas SET Persona = {0} WHERE Persona = {1}", nombreNuevo, nombreViejo);
        }

        public async Task RenombrarPersonaEnComprasAsync(string nombreViejo, string nombreNuevo)
        {
            // Por si en el futuro Compras tiene persona
        }

        public async Task SeedDefaultsAsync()
        {
            if (await _context.Personas.AnyAsync()) return;

            _context.Personas.AddRange(
                new PersonaEntity { Nombre = "Bubu", Orden = 0, FechaCreacion = DateTime.Now },
                new PersonaEntity { Nombre = "Dudu", Orden = 1, FechaCreacion = DateTime.Now }
            );
            await _context.SaveChangesAsync();
        }
    }
}
