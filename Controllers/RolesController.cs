using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VillexMVC.Models;

namespace VillexMVC.Controllers
{
    public class RolesController : Controller
    {
        private readonly IConfiguration _configuration;

        public RolesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection")!;
            return new SqlConnection(connStr);
        }

        private bool SesionActiva()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));
        }

        // ── LISTAR ─────────────────────────────────────
        public IActionResult Index()
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            var roles = new List<Rol>();
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "SELECT IdRol, Nombre, Descripcion FROM Roles ORDER BY IdRol";
                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    roles.Add(new Rol
                    {
                        IdRol       = (int)reader["IdRol"],
                        Nombre      = reader["Nombre"].ToString()!,
                        Descripcion = reader["Descripcion"].ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar roles: " + ex.Message;
            }

            ViewBag.Usuario = HttpContext.Session.GetString("Usuario");
            return View(roles);
        }

        // ── ALTA ────────────────────────────────────────
        [HttpGet]
        public IActionResult Crear()
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Rol rol)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                ViewBag.Error = "El nombre del rol es obligatorio.";
                return View(rol);
            }

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Roles WHERE Nombre = @Nombre";
                using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Nombre", rol.Nombre);
                int existe = (int)checkCmd.ExecuteScalar();

                if (existe > 0)
                {
                    ViewBag.Error = "Ya existe un rol con ese nombre.";
                    return View(rol);
                }

                string query = "INSERT INTO Roles (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", rol.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Descripcion", rol.Descripcion.Trim());
                cmd.ExecuteNonQuery();

                TempData["Exito"] = "Rol creado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(rol);
            }
        }

        // ── EDITAR ──────────────────────────────────────
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "SELECT IdRol, Nombre, Descripcion FROM Roles WHERE IdRol = @Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var rol = new Rol
                    {
                        IdRol       = (int)reader["IdRol"],
                        Nombre      = reader["Nombre"].ToString()!,
                        Descripcion = reader["Descripcion"].ToString()!
                    };
                    return View(rol);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(Rol rol)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(rol.Nombre))
            {
                ViewBag.Error = "El nombre del rol es obligatorio.";
                return View(rol);
            }

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "UPDATE Roles SET Nombre = @Nombre, Descripcion = @Descripcion WHERE IdRol = @IdRol";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", rol.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Descripcion", rol.Descripcion.Trim());
                cmd.Parameters.AddWithValue("@IdRol", rol.IdRol);
                cmd.ExecuteNonQuery();

                TempData["Exito"] = "Rol actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(rol);
            }
        }

        // ── ELIMINAR ────────────────────────────────────
        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "DELETE FROM Roles WHERE IdRol = @Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();

                TempData["Exito"] = "Rol eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
