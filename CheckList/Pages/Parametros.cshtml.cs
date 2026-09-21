using CheckList.Core.Parametro.Logic;
using CheckList.Core.Persona.DataAccess;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class ParametrosModel : PageModel
    {
        private readonly IPersonaRepository _personaRepository;
        private readonly IParametroService _parametroService;

        public List<PersonaDto> Personas { get; set; } = new();

        public string ClimaCiudad { get; set; } = "";
        public string ClimaProvincia { get; set; } = "";

        public ParametrosModel(IPersonaRepository personaRepository, IParametroService parametroService)
        {
            _personaRepository = personaRepository;
            _parametroService = parametroService;
        }

        public async Task OnGetAsync()
        {
            var personas = await _personaRepository.GetAllAsync();
            Personas = personas.Select(p => new PersonaDto
            {
                Id = p.Id,
                Nombre = p.Nombre
            }).ToList();

            ClimaCiudad = await _parametroService.ObtenerAsync("CLIMA_CIUDAD") ?? "Los Polvorines";

            ClimaProvincia = await _parametroService.ObtenerAsync("CLIMA_PROVINCIA") ?? "Buenos Aires";
        }
    }

    public class PersonaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
    }
}
