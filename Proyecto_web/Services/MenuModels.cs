namespace Proyecto_web.Models
{
    public class FormularioItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Ruta { get; set; } // ✅ nueva propiedad
    }

    public class SubmenuItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } // ✅ cambiar de Nombre a Titulo
        public List<FormularioItem> Formularios { get; set; } = new();
    }

    public class MenuItem
    {
        public int Id { get; set; }
        public string Titulo { get; set; } // ✅ cambiar de Nombre a Titulo
        public List<SubmenuItem> Submenus { get; set; } = new();
    }
}
