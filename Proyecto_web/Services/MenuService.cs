using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Proyecto_web.Models;

namespace Proyecto_web.Services
{
    public class MenuService
    {
        private readonly IConfiguration _configuration;

        public MenuService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<MenuItem> ObtenerMenus()
        {
            var menus = new List<MenuItem>();
            string connStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sql = @"
                    SELECT m.Id, m.Titulo, s.Id AS SubmenuId, s.Titulo AS SubmenuNombre, 
                           f.Id AS FormularioId, f.Nombre AS FormularioNombre ,f.Ruta
                    FROM Menu m
                    LEFT JOIN Submenu s ON s.MenuId = m.Id
                    LEFT JOIN Formulario f ON f.SubmenuId = s.Id
                    WHERE m.Status = 1 AND s.Status = 1 AND f.Status = 1
                    ORDER BY m.Id, s.Id, f.Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int menuId = reader.GetInt32(0);
                        string menuNombre = reader.GetString(1);

                        var menu = menus.FirstOrDefault(x => x.Id == menuId);
                        if (menu == null)
                        {
                            menu = new MenuItem
                            {
                                Id = menuId,
                                Titulo = menuNombre,
                                Submenus = new List<SubmenuItem>()
                            };
                            menus.Add(menu);
                        }

                        if (!reader.IsDBNull(2))
                        {
                            int submenuId = reader.GetInt32(2);
                            string submenuNombre = reader.GetString(3);

                            var submenu = menu.Submenus.FirstOrDefault(s => s.Id == submenuId);
                            if (submenu == null)
                            {
                                submenu = new SubmenuItem
                                {
                                    Id = submenuId,
                                    Titulo = submenuNombre,
                                    Formularios = new List<FormularioItem>()
                                };
                                menu.Submenus.Add(submenu);
                            }

                            if (!reader.IsDBNull(4))
                            {
                                submenu.Formularios.Add(new FormularioItem
                                {
                                    Id = reader.GetInt32(4),
                                    Titulo = reader.GetString(5),
                                    Ruta = reader.GetString(6)  // <-- esta línea es clave
                                }); 
                            }
                        }
                    }
                }
            }

            return menus;
        }
    }
}
