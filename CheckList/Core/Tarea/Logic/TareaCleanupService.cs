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

            if (lastCleanup != null && DateTime.TryParse(lastCleanup.Value, out var lastDate))
            {
                if (lastDate.Date == hoy)
                    return;
            }

            // 1. Resetear tareas diarias
            var tareasDiarias = await _tareaRepository.GetTareasDiariaAsync();
            foreach (var tarea in tareasDiarias)
            {
                tarea.Completada = false;
                await _tareaRepository.UpdateTareaAsync(tarea);
            }

            // 2. Mover tareas específicas no completadas: solo actualizamos Fecha a hoy.
            //    El atraso se calcula on-the-fly en el frontend como (hoy - Fecha).Days
            var tareasAtrasadas = await _tareaRepository.GetTareasAtrasadasAsync(hoy);
            foreach (var tarea in tareasAtrasadas)
            {
                // No tocamos Nombre ni Fecha — la fecha original ya indica cuántos días lleva atrasada
                // Solo nos aseguramos de que siga apareciendo en la lista de hoy
                // (GetTareasHoyAsync ya las incluye porque Fecha < hoy && !Completada)
                await _tareaRepository.UpdateTareaAsync(tarea);
            }

            // 3. Borrar tareas específicas completadas de días anteriores
            var tareasCompletadasViejas = await _tareaRepository.GetTareasEspecificasCompletadasAntesDeAsync(hoy);
            foreach (var tarea in tareasCompletadasViejas)
            {
                await _tareaRepository.DeleteTareaAsync(tarea.Id);
            }

            // 3.5. Borrar eventos cuya fecha fin ha pasado (NO se borran cumpleaños, se repiten anualmente)
            var eventosPasados = await _tareaRepository.GetEventosPasadosAsync(hoy);
            foreach (var evento in eventosPasados)
            {
                await _tareaRepository.DeleteTareaAsync(evento.Id);
            }

            // 4. Resetear tareas semanales completadas cuyo día vuelve a tocar hoy
            var diaHoy = hoy.DayOfWeek;

            // Primero, resetear las que ESTÁN completadas
            var tareasSemanalesCompletadas = await _tareaRepository.GetTareasSemanalesCompletadasAsync(hoy);
            foreach (var tarea in tareasSemanalesCompletadas)
            {
                tarea.Completada = false;
                tarea.Fecha = hoy;
                await _tareaRepository.UpdateTareaAsync(tarea);
            }

            // Luego, dejar las incompletas tal cual (solo aparecen atrasadas)
            var tareasSemanalesIncompletas = await _tareaRepository.GetTareasSemanalesAtrasadasAsync(hoy);
            foreach (var tarea in tareasSemanalesIncompletas)
            {
                // No tocamos Fecha ni Completada — la fecha original indica los días atrasada
                // Solo nos aseguramos de que siga en la BD
                await _tareaRepository.UpdateTareaAsync(tarea);
            }

            // 5. Guardar fecha de última limpieza
            await _settingRepository.SaveSettingAsync("LastCleanupDate", hoy.ToString("yyyy-MM-dd"));
        }
    }
}