using CheckList.Core.Tarea.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckList.Core.Tarea.DataAccess
{
    public interface IAppSettingRepository
    {
        Task<AppSetting> GetSettingAsync(string key);
        Task SaveSettingAsync(string key, string value);
    }

    public class AppSettingRepository : IAppSettingRepository
    {
        private readonly CheckListDbContext _context;

        public AppSettingRepository(CheckListDbContext context)
        {
            _context = context;
        }

        public async Task<AppSetting> GetSettingAsync(string key)
        {
            return await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            
            if (setting == null)
            {
                setting = new AppSetting { Key = key, Value = value, FechaActualizacion = DateTime.Now };
                _context.AppSettings.Add(setting);
            }
            else
            {
                setting.Value = value;
                setting.FechaActualizacion = DateTime.Now;
                _context.AppSettings.Update(setting);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}
