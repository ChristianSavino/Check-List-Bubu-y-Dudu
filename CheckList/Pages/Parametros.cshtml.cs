using CheckList.Core.Persona.DataAccess;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class ParametrosModel : PageModel
    {
        private readonly IPersonaRepository _personaRepository;

        public List<PersonaDto> Personas { get; set; } = new();

        public ParametrosModel(IPersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task OnGetAsync()
        {
            var personas = await _personaRepository.GetAllAsync();
            Personas = personas.Select(p => new PersonaDto
            {
                Id = p.Id,
                Nombre = p.Nombre
            }).ToList();
        }
    }

    public class PersonaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }
}
