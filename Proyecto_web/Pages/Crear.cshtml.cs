using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Proyecto_web.Pages
{
    public class CrearModel : PageModel
    {
        private readonly IConfiguration _config;
        public CrearModel(IConfiguration config) => _config = config;

        [BindProperty, Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50)]
        public string NombreUsuario { get; set; }

        [BindProperty, Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Correo { get; set; }

        [BindProperty, Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(50, MinimumLength = 4)]
        public string Password { get; set; }

        public string MensajeExito { get; set; }
        public string MensajeError { get; set; }

        public List<UsuarioDTO> ListaUsuarios { get; set; } = new();
        public bool IrAlPanel { get; set; }

        public class UsuarioDTO
        {
            public int Id { get; set; }
            public string NombreUsuario { get; set; }
            public string Correo { get; set; }
            public int Estado { get; set; }
            public bool EnSesion { get; set; }
        }

        public void OnGet()
        {
            CargarUsuarios();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                CargarUsuarios();
                return Page();
            }

            var partes = NombreUsuario.Trim().Split(' ');
            if (partes.Length < 2)
            {
                MensajeError = "Debe ingresar nombre y apellido.";
                CargarUsuarios();
                return Page();
            }

            string nombreSimple = partes[0];
            string apellido = partes[1];
            string nombreGenerado = (nombreSimple[0] + apellido).ToLower();

            string connStr = _config.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connStr);
            conn.Open();

            string query = @"
                INSERT INTO Usuarios (Nombre, Nombre_Usuario, Correo, Contraseña)
                VALUES (@nombre, @nombre_usuario, @correo, @pass)";

            using SqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@nombre", nombreGenerado);
            cmd.Parameters.AddWithValue("@nombre_usuario", NombreUsuario);
            cmd.Parameters.AddWithValue("@correo", Correo);
            cmd.Parameters.AddWithValue("@pass", Password);

            try
            {
                cmd.ExecuteNonQuery();
                MensajeExito = "Usuario registrado correctamente.";
                IrAlPanel = true; // mostrar panel tras registro
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                MensajeError = "Error al registrar: " + ex.Message;
            }

            CargarUsuarios();
            return Page();
        }

        private void CargarUsuarios()
        {
            ListaUsuarios = new List<UsuarioDTO>();
            string connStr = _config.GetConnectionString("DefaultConnection");

            using SqlConnection conn = new(connStr);
            conn.Open();

            string query = "SELECT Id, Nombre_Usuario, Correo, Estado, EnSesion FROM Usuarios";

            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ListaUsuarios.Add(new UsuarioDTO
                {
                    Id = reader.GetInt32(0),
                    NombreUsuario = reader.GetString(1),
                    Correo = reader.IsDBNull(2) ? "(sin correo)" : reader.GetString(2),
                    Estado = reader.GetInt32(3),
                    EnSesion = reader.GetBoolean(4)
                });
            }
        }

        // Opcionales: baja, cierre de sesión, etc.
        public IActionResult OnPostBaja(int id)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connStr);
            conn.Open();

            string sql = "UPDATE Usuarios SET Estado = 2 WHERE Id = @id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            MensajeExito = "Usuario dado de baja.";
            CargarUsuarios();
            IrAlPanel = true;
            return Page();
        }

        public IActionResult OnPostCerrarSesion(int id)
        {
            string connStr = _config.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connStr);
            conn.Open();

            string sql = "UPDATE Usuarios SET EnSesion = 0 WHERE Id = @id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            MensajeExito = "Sesión cerrada correctamente.";
            CargarUsuarios();
            IrAlPanel = true;
            return Page();
        }

        public IActionResult OnPostResetPassword(int id)
        {
            string nueva = "123456";
            string connStr = _config.GetConnectionString("DefaultConnection");
            using SqlConnection conn = new(connStr);
            conn.Open();

            string sql = "UPDATE Usuarios SET Contraseña = @nueva WHERE Id = @id";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@nueva", nueva);
            cmd.ExecuteNonQuery();

            MensajeExito = "Contraseña restablecida.";
            CargarUsuarios();
            IrAlPanel = true;
            return Page();
        }
    }
}
