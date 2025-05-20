using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;

public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public string Nombre { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Debe ingresar usuario y contraseña.";
            return Page();
        }

        string connStr = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();

            var cmd = new SqlCommand(@"
            SELECT Id, Nombre_Usuario, Estado, EnSesion 
            FROM Usuarios 
            WHERE Nombre = @nombre AND Contraseña = @pass", conn);

            cmd.Parameters.AddWithValue("@nombre", Nombre);
            cmd.Parameters.AddWithValue("@pass", Password);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int userId = reader.GetInt32(0);
                    string nombreUsuario = reader.GetString(1);
                    int estado = reader.GetInt32(2);
                    bool enSesion = reader.GetBoolean(3);

                    // Verificar si está activo (excepto manager)
                    if (Nombre.ToLower() != "manager" && estado != 1)
                    {
                        ErrorMessage = "Usuario desactivado. Contacte al administrador.";
                        return Page();
                    }

                    // Evitar doble sesión (excepto manager)
                    if (enSesion && Nombre.ToLower() != "manager")
                    {
                        ErrorMessage = "Este usuario ya tiene una sesión activa.";
                        return Page();
                    }

                    // Guardar sesión
                    HttpContext.Session.SetInt32("IdUsuario", userId);
                    HttpContext.Session.SetString("Nombre_Usuario", nombreUsuario);

                    reader.Close();

                    // Marcar como conectado
                    var updateCmd = new SqlCommand("UPDATE Usuarios SET EnSesion = 1 WHERE Id = @id", conn);
                    updateCmd.Parameters.AddWithValue("@id", userId);
                    updateCmd.ExecuteNonQuery();

                    return RedirectToPage("/Inicio");
                }
                else
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    return Page();
                }
            }
        }
    }

}
