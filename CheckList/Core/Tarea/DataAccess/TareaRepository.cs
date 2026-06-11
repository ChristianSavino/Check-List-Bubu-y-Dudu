using CheckList.Core.Tarea.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckList.Core.Tarea.DataAccess
{
    public interface ITareaRepository
    {
        Task<List<TareaEntity>> GetTareasAsync();
        Task<List<TareaEntity>> GetTareasDiariaAsync();
        Task<List<TareaEntity>> GetTareasHoyAsync(DateTime fecha);
        Task<List<TareaEntity>> GetTareasMañanaAsync(DateTime fecha);
        Task<List<TareaEntity>> GetTareasSemanalesHoyAsync(DateTime fecha);
        Task<List<TareaEntity>> GetTareasSemanalesMañanaAsync(DateTime fecha);
        Task<TareaEntity?> GetTareaByIdAsync(int id);
        Task AddTareaAsync(TareaEntity tarea);
        Task UpdateTareaAsync(TareaEntity tarea);
        Task DeleteTareaAsync(int id);
        Task<List<TareaEntity>> GetTareasAtrasadasAsync(DateTime hoy);
        Task<List<TareaEntity>> GetTareasSemanalesAtrasadasAsync(DateTime hoy);
        Task<List<TareaEntity>> GetTareasSemanalesCompletadasAsync(DateTime hoy);
        Task<List<TareaEntity>> GetTareasEspecificasCompletadasAntesDeAsync(DateTime fecha);
        Task<List<TareaEntity>> GetEventosActivosEnFechaAsync(DateTime fecha);
        Task<List<TareaEntity>> GetEventosPasadosAsync(DateTime hoy);
    }

    public class TareaRepository : ITareaRepository
    {
        private readonly CheckListDbContext _context;

        public TareaRepository(CheckListDbContext context)
        {
            _context = context;
        }

        public async Task<List<TareaEntity>> GetTareasAsync()
        {
            return await _context.Tareas.ToListAsync();
        }

        public async Task<List<TareaEntity>> GetTareasDiariaAsync()
        {
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Daily)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        public async Task<List<TareaEntity>> GetTareasHoyAsync(DateTime fecha)
        {
            var fechaHoy = fecha.Date;

            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Specific
                         && t.Fecha.HasValue
                         && (
                                t.Fecha.Value.Date == fechaHoy
                                ||
                                (
                                    t.Fecha.Value.Date < fechaHoy
                                    && !t.Completada
                                )
                            ))
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.Orden)
                .ToListAsync();
        }

        public async Task<List<TareaEntity>> GetTareasMañanaAsync(DateTime fecha)
        {
            var fechaMañana = fecha.AddDays(1).Date;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Specific
                         && t.Fecha.HasValue
                         && t.Fecha.Value.Date == fechaMañana)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        // Tareas weekly cuyo DiaSemana coincide con hoy,
        // O que están atrasadas (Fecha < hoy y no completadas)
        public async Task<List<TareaEntity>> GetTareasSemanalesHoyAsync(DateTime fecha)
        {
            var fechaHoy = fecha.Date;
            var diaHoy = fecha.DayOfWeek;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Weekly
                         && t.DiaSemana.HasValue
                         && (t.DiaSemana.Value == diaHoy
                             || (t.Fecha.HasValue && t.Fecha.Value.Date < fechaHoy && !t.Completada)))
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        // Tareas weekly cuyo DiaSemana coincide con mañana (solo las no atrasadas)
        public async Task<List<TareaEntity>> GetTareasSemanalesMañanaAsync(DateTime fecha)
        {
            var fechaMañana = fecha.AddDays(1).Date;
            var diaMañana = fechaMañana.DayOfWeek;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Weekly
                         && t.DiaSemana.HasValue
                         && t.DiaSemana.Value == diaMañana)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        public async Task<List<TareaEntity>> GetTareasAtrasadasAsync(DateTime hoy)
        {
            var fechaHoy = hoy.Date;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Specific
                         && t.Fecha.HasValue
                         && t.Fecha.Value.Date < fechaHoy
                         && !t.Completada)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        // Weekly no completadas cuya última fecha asignada fue antes de hoy
        // (ya manejadas en GetTareasSemanalesHoyAsync, pero necesarias para el cleanup)
        public async Task<List<TareaEntity>> GetTareasSemanalesAtrasadasAsync(DateTime hoy)
        {
            var fechaHoy = hoy.Date;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Weekly
                         && t.Fecha.HasValue
                         && t.Fecha.Value.Date < fechaHoy
                         && !t.Completada)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        // Weekly completadas cuya última fecha asignada fue antes de hoy
        // (necesarias para resetear el ciclo cuando el día vuelve a tocar)
        public async Task<List<TareaEntity>> GetTareasSemanalesCompletadasAsync(DateTime hoy)
        {
            var diaHoy = hoy.DayOfWeek;

            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Weekly
                         && t.Completada
                         && t.DiaSemana.HasValue
                         && t.DiaSemana.Value == diaHoy)
                .OrderBy(t => t.Orden)
                .ToListAsync();
        }

        public async Task<TareaEntity?> GetTareaByIdAsync(int id)
        {
            return await _context.Tareas.FindAsync(id);
        }

        public async Task AddTareaAsync(TareaEntity tarea)
        {
            tarea.FechaCreacion = DateTime.Now;
            tarea.FechaActualizacion = DateTime.Now;
            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTareaAsync(TareaEntity tarea)
        {
            tarea.FechaActualizacion = DateTime.Now;
            _context.Tareas.Update(tarea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTareaAsync(int id)
        {
            var tarea = await _context.Tareas.FindAsync(id);
            if (tarea != null)
            {
                _context.Tareas.Remove(tarea);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<TareaEntity>> GetTareasEspecificasCompletadasAntesDeAsync(DateTime fecha)
        {
            var fechaHoy = fecha.Date;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Specific
                         && t.Completada
                         && t.Fecha.HasValue
                         && t.Fecha.Value.Date < fechaHoy)
                .ToListAsync();
        }

        public async Task<List<TareaEntity>> GetEventosActivosEnFechaAsync(DateTime fecha)
        {
            var fechaDia = fecha.Date;
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Event
                         && t.Fecha.HasValue && t.FechaFin.HasValue
                         && t.Fecha.Value.Date <= fechaDia
                         && t.FechaFin.Value.Date >= fechaDia)
                .OrderBy(t => t.Fecha)
                .ToListAsync();
        }

        public async Task<List<TareaEntity>> GetEventosPasadosAsync(DateTime hoy)
        {
            return await _context.Tareas
                .Where(t => t.Tipo == TipoTarea.Event
                         && t.FechaFin.HasValue
                         && t.FechaFin.Value.Date < hoy.Date)
                .ToListAsync();
        }
    }
}