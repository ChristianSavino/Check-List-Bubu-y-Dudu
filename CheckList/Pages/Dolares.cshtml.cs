using CheckList.Core.Dolares.Domain;
using CheckList.Core.Dolares.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class DolaresModel : PageModel
    {
        private readonly IDolaresService _dolaresService;

        public Cotizacion Cotizacion { get; set; } = new();

        public DolaresModel(IDolaresService dolaresService)
        {
            _dolaresService = dolaresService;
        }

        public async Task OnGet()
        {
            Cotizacion = await _dolaresService.ObtenerCotizaciones();
        }
    }
}
