using CheckList.Core.Clima.Domain;
using CheckList.Core.Clima.Logic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class ClimaModel : PageModel
    {
        private readonly IUserClimaService _userClimaService;

        public ClimaResult? Clima { get; private set; }

        public string? Error { get; private set; }

        public ClimaModel(IUserClimaService userClimaService)
        {
            _userClimaService = userClimaService;
        }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            try
            {
                Clima = await _userClimaService.ObtenerClimaAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
