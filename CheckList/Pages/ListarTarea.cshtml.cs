using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CheckList.Pages
{
    public class ListarTareaModel : PageModel
    {
        public List<TareaDTO> Tareas { get; set; } = new();

        public void OnGet()
        {
            CargarTareas();
        }

        public IActionResult OnPostDelete(int id)
        {
            // Aquí se eliminaría la tarea de la BD
            CargarTareas();
            return RedirectToPage();
        }

        private void CargarTareas()
        {
            Tareas = new List<TareaDTO>
            {
                new() { 
                    Id = 1, 
                    Nombre = "Regar plantas", 
                    Tipo = "daily", 
                    TipoLabel = "Diaria",
                    Fecha = null,
                    Hora = "",
                    Persona = "Bubu"
                },
                new() { 
                    Id = 2, 
                    Nombre = "Sacar basura", 
                    Tipo = "daily", 
                    TipoLabel = "Diaria",
                    Fecha = null,
                    Hora = "",
                    Persona = ""
                },
                new() { 
                    Id = 3, 
                    Nombre = "Revisar correos", 
                    Tipo = "daily", 
                    TipoLabel = "Diaria",
                    Fecha = null,
                    Hora = "",
                    Persona = "Dudu"
                },
                new() { 
                    Id = 4, 
                    Nombre = "Pagar impuesto", 
                    Tipo = "specific", 
                    TipoLabel = "Específica",
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                    Hora = "10:00",
                    Persona = "Bubu"
                },
                new() { 
                    Id = 5, 
                    Nombre = "Bañar al perro", 
                    Tipo = "specific", 
                    TipoLabel = "Específica",
                    Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                    Hora = "16:30",
                    Persona = ""
                },
                new() { 
                    Id = 6, 
                    Nombre = "Llamar a Rigorberto", 
                    Tipo = "specific", 
                    TipoLabel = "Específica",
                    Fecha = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    Hora = "",
                    Persona = ""
                },
                new() { 
                    Id = 7, 
                    Nombre = "Pagar tarjeta", 
                    Tipo = "specific", 
                    TipoLabel = "Específica",
                    Fecha = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    Hora = "12:00",
                    Persona = "Dudu"
                }
            };
        }
    }

    public class TareaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string TipoLabel { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Persona { get; set; }
    }
}
