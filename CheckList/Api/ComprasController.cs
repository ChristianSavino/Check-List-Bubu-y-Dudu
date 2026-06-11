using CheckList.Core.Compra.DataAccess;
using CheckList.Core.Compra.Domain;
using CheckList.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CheckList.Api
{
    [ApiController]
    [Route("api/compras")]
    public class ComprasController : ControllerBase
    {
        private readonly ICompraRepository _repository;
        private readonly IHubContext<ChecklistHub> _hubContext;

        public ComprasController(ICompraRepository repository, IHubContext<ChecklistHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CompraAddRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                return BadRequest();

            var compra = new CompraEntity
            {
                Nombre = request.Nombre.Trim(),
                Tipo = Enum.Parse<TipoCompra>(request.Tipo, ignoreCase: true),
                Completada = false
            };

            await _repository.AddAsync(compra);

            await _hubContext.Clients.Group("checklist").SendAsync("ComprasUpdated");

            return Ok(new { id = compra.Id, nombre = compra.Nombre });
        }

        [HttpPost("toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            var compra = await _repository.GetByIdAsync(id);
            if (compra == null) return NotFound();

            compra.Completada = !compra.Completada;
            await _repository.UpdateAsync(compra);

            await _hubContext.Clients.Group("checklist").SendAsync("ComprasUpdated");

            return Ok(new { id = compra.Id, completada = compra.Completada });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            await _hubContext.Clients.Group("checklist").SendAsync("ComprasUpdated");
            return Ok();
        }

        [HttpPost("clear/{tipo}")]
        public async Task<IActionResult> Clear(string tipo)
        {
            var tipoCompra = Enum.Parse<TipoCompra>(tipo, ignoreCase: true);
            await _repository.ClearByTipoAsync(tipoCompra);

            await _hubContext.Clients.Group("checklist").SendAsync("ComprasUpdated");

            return Ok();
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();

            await _repository.ReorderAsync(ids);
            await _hubContext.Clients.Group("checklist").SendAsync("ComprasUpdated");
            return Ok();
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var diarias = await _repository.GetByTipoAsync(TipoCompra.Diaria);
            var otras = await _repository.GetByTipoAsync(TipoCompra.Otra);

            return Ok(new
            {
                diarias = diarias.Select(c => new { c.Id, c.Nombre, c.Completada, c.Orden }),
                otras = otras.Select(c => new { c.Id, c.Nombre, c.Completada, c.Orden })
            });
        }
    }

    public class CompraAddRequest
    {
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "Diaria";
    }
}
