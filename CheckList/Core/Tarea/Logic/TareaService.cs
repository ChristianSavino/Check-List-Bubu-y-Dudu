using CheckList.Core.Tarea.DataAccess;
using CheckList.Core.Tarea.Domain;

namespace CheckList.Core.Tarea.Logic
{
    public interface ITareaService
    {
        Task<List<TareaEntity>> GetTareasHoyAsync();
        Task<List<TareaEntity>> GetTareasDiariaAsync();
        Task<List<TareaEntity>> GetTareasMañanaAsync();
        Task<List<TareaEntity>> GetTareasSemanalesHoyAsync();
        Task<List<TareaEntity>> GetTareasSemanalesMañanaAsync();
        Task<List<TareaEntity>> GetTodasLasTareasAsync();
        Task<TareaEntity?> GetTareaByIdAsync(int id);
        Task CrearTareaAsync(TareaEntity tarea);
        Task ActualizarTareaAsync(TareaEntity tarea);
        Task EliminarTareaAsync(int id);
        Task ToggleTareaAsync(int id);
        Task ReordenarTareasAsync(List<int> ids);
        Task<List<TareaEntity>> GetEventosActivosEnFechaAsync(DateTime fecha);
    }

    public class TareaService : ITareaService
    {
        private readonly ITareaRepository _repository;

        public TareaService(ITareaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TareaEntity>> GetTareasHoyAsync()
            => await _repository.GetTareasHoyAsync(DateTime.Now);

        public async Task<List<TareaEntity>> GetTareasDiariaAsync()
            => await _repository.GetTareasDiariaAsync();

        public async Task<List<TareaEntity>> GetTareasMañanaAsync()
            => await _repository.GetTareasMañanaAsync(DateTime.Now);

        public async Task<List<TareaEntity>> GetTareasSemanalesHoyAsync()
            => await _repository.GetTareasSemanalesHoyAsync(DateTime.Now);

        public async Task<List<TareaEntity>> GetTareasSemanalesMañanaAsync()
            => await _repository.GetTareasSemanalesMañanaAsync(DateTime.Now);

        public async Task<List<TareaEntity>> GetTodasLasTareasAsync()
            => await _repository.GetTareasAsync();

        public async Task<TareaEntity?> GetTareaByIdAsync(int id)
            => await _repository.GetTareaByIdAsync(id);

        public async Task CrearTareaAsync(TareaEntity tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.Nombre))
                throw new ArgumentException("El nombre de la tarea es requerido");

            if (tarea.Tipo == TipoTarea.Specific && !tarea.Fecha.HasValue)
                throw new ArgumentException("Las tareas específicas requieren una fecha");

            if (tarea.Tipo == TipoTarea.Weekly && !tarea.DiaSemana.HasValue)
                throw new ArgumentException("Las tareas semanales requieren un día de la semana");

            if (tarea.Tipo == TipoTarea.Event && (!tarea.Fecha.HasValue || !tarea.FechaFin.HasValue))
                throw new ArgumentException("Los eventos requieren fecha de inicio y fin");

            await _repository.AddTareaAsync(tarea);
        }

        public async Task ActualizarTareaAsync(TareaEntity tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.Nombre))
                throw new ArgumentException("El nombre de la tarea es requerido");

            if (tarea.Tipo == TipoTarea.Specific && !tarea.Fecha.HasValue)
                throw new ArgumentException("Las tareas específicas requieren una fecha");

            if (tarea.Tipo == TipoTarea.Weekly && !tarea.DiaSemana.HasValue)
                throw new ArgumentException("Las tareas semanales requieren un día de la semana");
            
            if (tarea.Tipo == TipoTarea.Event && (!tarea.Fecha.HasValue || !tarea.FechaFin.HasValue))
                throw new ArgumentException("Los eventos requieren fecha de inicio y fin");

            await _repository.UpdateTareaAsync(tarea);
        }

        public async Task EliminarTareaAsync(int id)
            => await _repository.DeleteTareaAsync(id);

        public async Task ToggleTareaAsync(int id)
        {
            var tarea = await _repository.GetTareaByIdAsync(id);
            if (tarea != null)
            {
                tarea.Completada = !tarea.Completada;
                await _repository.UpdateTareaAsync(tarea);
            }
        }

        public async Task ReordenarTareasAsync(List<int> ids)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                var tarea = await _repository.GetTareaByIdAsync(ids[i]);
                if (tarea != null)
                {
                    tarea.Orden = i;
                    await _repository.UpdateTareaAsync(tarea);
                }
            }
        }

        public async Task<List<TareaEntity>> GetEventosActivosEnFechaAsync(DateTime fecha)
            => await _repository.GetEventosActivosEnFechaAsync(fecha);
    }
}