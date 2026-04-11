using CheckList.Core.Tarea.DataAccess;

namespace CheckList.Core.Tarea.Logic
{
    public interface ITareaCleanupService
    {
        Task CleanupAsync();
    }

    public class TareaCleanupService : ITareaCleanupService
    {
        private readonly ITareaRepository _tareaRepository;
        private readonly IAppSettingRepository _settingRepository;

        public TareaCleanupService(ITareaRepository tareaRepository, IAppSettingRepository settingRepository)
        {
            _tareaRepository = tareaRepository;
            _settingRepository = settingRepository;
        }

        public async Task CleanupAsync()
        {
            var hoy = DateTime.Now.Date;
            var lastCleanup = await _settingRepository.GetSettingAsync("LastCleanupDate");
            
            // Si ya se ejecutó hoy, no hacer nada
            if (lastCleanup != null && DateTime.TryParse(lastCleanup.Value, out var lastDate))
            {
                if (lastDate.Date == hoy)
                {
                    return;
                }
            }

            // 1. Resetear checkboxes de tareas diarias
            var tareasDiarias = await _tareaRepository.GetTareasDiariaAsync();
            foreach (var tarea in tareasDiarias)
            {
                tarea.Completada = false;
                await _tareaRepository.UpdateTareaAsync(tarea);
            }

            // 2. Mover tareas no completadas del día anterior a hoy con etiqueta "atrasado"
            var ayer = hoy.AddDays(-1);
            var tareasAtrasadas = await _tareaRepository.GetTareasAtrasadasAsync(hoy);
            
            foreach (var tarea in tareasAtrasadas)
            {
                // Calcular cuántos días atrás está
                var diasAtraso = (hoy - tarea.Fecha.Value.Date).Days;
                tarea.Nombre = $"{tarea.Nombre} (atrasado {diasAtraso} días)";
                tarea.Fecha = hoy;
                tarea.Completada = false;
                await _tareaRepository.UpdateTareaAsync(tarea);
            }


            // 3. Borrar tareas específicas completadas de días anteriores
            var tareasCompletadasViejas = await _tareaRepository.GetTareasEspecificasCompletadasAntesDeAsync(hoy);
            foreach (var tarea in tareasCompletadasViejas)
            {
                await _tareaRepository.DeleteTareaAsync(tarea.Id);
            }

            // 4. Guardar fecha de última limpieza
            await _settingRepository.SaveSettingAsync("LastCleanupDate", hoy.ToString("yyyy-MM-dd"));
        }
    }
}
