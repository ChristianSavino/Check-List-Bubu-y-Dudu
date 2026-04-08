using CheckList.Core.Tarea.DataAccess;
using CheckList.Core.Tarea.Domain;

namespace CheckList.Core.Tarea.Logic
{
    public interface ITareaService
    {
        Task<List<TareaEntity>> GetTareasHoyAsync();
        Task<List<TareaEntity>> GetTareasDiariaAsync();
        Task<List<TareaEntity>> GetTareasMañanaAsync();
        Task<List<TareaEntity>> GetTodasLasTareasAsync();
        Task<TareaEntity> GetTareaByIdAsync(int id);
        Task CrearTareaAsync(TareaEntity tarea);
        Task ActualizarTareaAsync(TareaEntity tarea);
        Task EliminarTareaAsync(int id);
        Task ToggleTareaAsync(int id);
    }

    public class TareaService : ITareaService
    {
        private readonly ITareaRepository _repository;

        public TareaService(ITareaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TareaEntity>> GetTareasHoyAsync()
        {
            return await _repository.GetTareasHoyAsync(DateTime.Now);
        }

        public async Task<List<TareaEntity>> GetTareasDiariaAsync()
        {
            return await _repository.GetTareasDiariaAsync();
        }

        public async Task<List<TareaEntity>> GetTareasMañanaAsync()
        {
            return await _repository.GetTareasMañanaAsync(DateTime.Now);
        }

        public async Task<List<TareaEntity>> GetTodasLasTareasAsync()
        {
            return await _repository.GetTareasAsync();
        }

        public async Task<TareaEntity> GetTareaByIdAsync(int id)
        {
            return await _repository.GetTareaByIdAsync(id);
        }

        public async Task CrearTareaAsync(TareaEntity tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.Nombre))
                throw new ArgumentException("El nombre de la tarea es requerido");

            if (tarea.Tipo == "specific" && !tarea.Fecha.HasValue)
                throw new ArgumentException("Las tareas específicas requieren una fecha");

            await _repository.AddTareaAsync(tarea);
        }

        public async Task ActualizarTareaAsync(TareaEntity tarea)
        {
            if (string.IsNullOrWhiteSpace(tarea.Nombre))
                throw new ArgumentException("El nombre de la tarea es requerido");

            if (tarea.Tipo == "specific" && !tarea.Fecha.HasValue)
                throw new ArgumentException("Las tareas específicas requieren una fecha");

            await _repository.UpdateTareaAsync(tarea);
        }

        public async Task EliminarTareaAsync(int id)
        {
            await _repository.DeleteTareaAsync(id);
        }

        public async Task ToggleTareaAsync(int id)
        {
            var tarea = await _repository.GetTareaByIdAsync(id);
            if (tarea != null)
            {
                tarea.Completada = !tarea.Completada;
                await _repository.UpdateTareaAsync(tarea);
            }
        }
    }
}
