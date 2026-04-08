using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace CheckList.Pages
{
    public class AgregarTareaModel : PageModel
    {
        [BindProperty]
        public string Nombre { get; set; }

        [BindProperty]
        public string Tipo { get; set; } = "daily";

        [BindProperty]
        public string Fecha { get; set; }

        [BindProperty]
        public string Hora { get; set; }

        [BindProperty]
        public string Persona { get; set; } = "";

        public int? TareaId { get; set; }
        public bool IsEditing { get; set; }
        public string PageTitle { get; set; } = "Agregar Tarea";

        public void OnGet(int? id)
        {
            TareaId = id;
            IsEditing = id.HasValue;
            
            if (IsEditing)
            {
                PageTitle = "Modificar Tarea";
                // Aquí se cargaría la tarea de la BD
                // Por ahora datos de ejemplo
                Nombre = "Ejemplo de tarea";
                Tipo = "specific";
                Fecha = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                Hora = "14:30";
                Persona = "Bubu";
            }
        }

        public IActionResult OnPost(int? id)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Nombre))
            {
                return Page();
            }

            // Validar que tarea específica tiene fecha
            if (Tipo == "specific" && string.IsNullOrWhiteSpace(Fecha))
            {
                ModelState.AddModelError("Fecha", "La fecha es obligatoria para tareas específicas");
                return Page();
            }

            if (id.HasValue)
            {
                // Actualizar tarea en BD
            }
            else
            {
                // Crear nueva tarea en BD
            }

            return RedirectToPage("/Index");
        }
    }
}
