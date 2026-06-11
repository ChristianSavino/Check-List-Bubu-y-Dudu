using CheckList.Core.Compra.DataAccess;
using CheckList.Core.Compra.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class ComprasModel : PageModel
    {
        private readonly ICompraRepository _repository;

        public List<CompraDto> Diarias { get; set; } = new();
        public List<CompraDto> Otras { get; set; } = new();

        public ComprasModel(ICompraRepository repository)
        {
            _repository = repository;
        }

        public async Task OnGetAsync()
        {
            var diarias = await _repository.GetByTipoAsync(TipoCompra.Diaria);
            var otras = await _repository.GetByTipoAsync(TipoCompra.Otra);

            Diarias = diarias.Select(c => new CompraDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Completada = c.Completada
            }).ToList();

            Otras = otras.Select(c => new CompraDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Completada = c.Completada
            }).ToList();
        }
    }

    public class CompraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public bool Completada { get; set; }
    }
}
