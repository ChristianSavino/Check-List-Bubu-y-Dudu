using CheckList.Core.Persona.DataAccess;
using CheckList.Core.Persona.Domain;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Api
{
    [ApiController]
    [Route("api/personas")]
    public class PersonasController : ControllerBase
    {
        private readonly IPersonaRepository _repository;
        private readonly IHubContext<ChecklistHub> _hubContext;
        private readonly ILogger<PersonasController> _logger;

        public PersonasController(IPersonaRepository repository, IHubContext<ChecklistHub> hubContext, ILogger<PersonasController> logger)
        {
            _repository = repository;
            _hubContext = hubContext;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var personas = await _repository.GetAllAsync();
            return Ok(personas.Select(p => new { p.Id, p.Nombre }));
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] PersonaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                return BadRequest("El nombre es obligatorio");

            var nombre = request.Nombre.Trim();
            var existente = await _repository.GetByNombreAsync(nombre);
            if (existente != null)
                return BadRequest("Ya existe una persona con ese nombre");

            var persona = new PersonaEntity { Nombre = nombre };
            await _repository.AddAsync(persona);

            await NotificarCambios();
            return Ok(new { persona.Id, persona.Nombre });
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PersonaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                return BadRequest("El nombre es obligatorio");

            var persona = await _repository.GetByIdAsync(id);
            if (persona == null) return NotFound();

            var nombreNuevo = request.Nombre.Trim();

            // Si cambió el nombre, actualizar tareas asignadas
            if (persona.Nombre != nombreNuevo)
            {
                var duplicado = await _repository.GetByNombreAsync(nombreNuevo);
                if (duplicado != null)
                    return BadRequest("Ya existe una persona con ese nombre");

                await _repository.RenombrarPersonaEnTareasAsync(persona.Nombre, nombreNuevo);
                persona.Nombre = nombreNuevo;
            }

            await _repository.UpdateAsync(persona);

            await NotificarCambios();
            return Ok(new { persona.Id, persona.Nombre });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var persona = await _repository.GetByIdAsync(id);
            if (persona == null) return NotFound();

            await _repository.DeleteAsync(id);

            await NotificarCambios();
            return Ok();
        }

        private async Task NotificarCambios()
        {
            await _hubContext.Clients.Group("checklist").SendAsync("PersonasUpdated");
            await _hubContext.Clients.Group("checklist").SendAsync("TasksUpdated");
        }
    }

    public class PersonaRequest
    {
        public string Nombre { get; set; } = "";
    }
}
