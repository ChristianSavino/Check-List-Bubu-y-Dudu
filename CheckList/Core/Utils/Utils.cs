using CheckList.Core.Tarea.Domain;

namespace CheckList.Core.Utils
{
    public static class Utils
    {
        public static string TransformarTipoTarea(TipoTarea tipo)
        {
            return tipo switch
            {
                TipoTarea.Daily => "Diaria",
                TipoTarea.Specific => "Específica",
                TipoTarea.Weekly => "Semanal",
                _ => "Desconocida"
            };
        }
    }
}
