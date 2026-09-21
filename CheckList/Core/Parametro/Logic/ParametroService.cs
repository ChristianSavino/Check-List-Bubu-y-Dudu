using CheckList.Core.Tarea.DataAccess;
using CheckList.Core.Tarea.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckList.Core.Parametro.Logic
{
    public interface IParametroService
    {
        Task<string?> ObtenerAsync(string clave);
        Task GuardarAsync(string clave, string valor);
    }

    public class ParametroService : IParametroService
    {
        private readonly CheckListDbContext _context;

        public ParametroService(CheckListDbContext context)
        {
            _context = context;
        }

        public async Task<string?> ObtenerAsync(string key)
        {
            return await _context.AppSettings
                .AsNoTracking()
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
        }

        public async Task GuardarAsync(string clave, string valor)
        {
            var parametro = await _context.AppSettings
                .FirstOrDefaultAsync(x => x.Key == clave);

            if (parametro == null)
            {
                parametro = new AppSetting
                {
                    Key = clave,
                    Value = valor
                };

                _context.AppSettings.Add(parametro);
            }
            else
            {
                parametro.Value = valor;
            }

            await _context.SaveChangesAsync();
        }
    }
}
