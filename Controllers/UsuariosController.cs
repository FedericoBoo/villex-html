using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using VillexMVC.Models;

namespace VillexMVC.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsuariosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            string connStr = _configuration.GetConnectionString("DefaultConnection")!;
            return new SqlConnection(connStr);
        }

        private bool SesionActiva() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("Usuario"));

        public IActionResult Index()
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            var usuarios = new List<UsuarioViewModel>();
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "SELECT IdUsuario, Usuario FROM Usuarios ORDER BY IdUsuario";
                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new UsuarioViewModel
                    {
                        Id = (int)reader["IdUsuario"],
                        Username = reader["Usuario"].ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar usuarios: " + ex.Message;
            }

            return View(usuarios);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");
            return View();
        }

        [HttpPost]
        public IActionResult Crear(NuevoUsuarioViewModel model)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                ViewBag.Error = "Usuario y contraseña son obligatorios.";
                return View(model);
            }

            if (!Regex.IsMatch(model.Username.Trim(), @"^[a-zA-ZÁÉÍÓÚáéíóúÑñ\s\.]+$"))
            {
                ViewBag.Error = "El nombre de usuario no puede contener números ni símbolos, solo letras.";
                return View(model);
            }

            if (model.Password != model.ConfirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View(model);
            }

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string check = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario";
                using SqlCommand checkCmd = new SqlCommand(check, conn);
                checkCmd.Parameters.AddWithValue("@Usuario", model.Username.Trim());
                if ((int)checkCmd.ExecuteScalar() > 0)
                {
                    ViewBag.Error = "Ya existe un usuario con ese nombre.";
                    return View(model);
                }

                string query = "INSERT INTO Usuarios (Usuario, Contrasena) VALUES (@Usuario, @Contrasena)";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Usuario", model.Username.Trim());
                cmd.Parameters.AddWithValue("@Contrasena", model.Password);
                cmd.ExecuteNonQuery();

                TempData["Exito"] = "Usuario creado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            UsuarioViewModel? usuario = null;
            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "SELECT IdUsuario, Usuario FROM Usuarios WHERE IdUsuario = @Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new UsuarioViewModel
                    {
                        Id = (int)reader["IdUsuario"],
                        Username = reader["Usuario"].ToString()!
                    };
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar usuario: " + ex.Message;
            }

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Index");
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioViewModel model)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ViewBag.Error = "El nombre de usuario es obligatorio.";
                return View(model);
            }

            if (!Regex.IsMatch(model.Username.Trim(), @"^[a-zA-ZÁÉÍÓÚáéíóúÑñ\s\.]+$"))
            {
                ViewBag.Error = "El nombre de usuario no puede contener números ni símbolos, solo letras.";
                return View(model);
            }

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();

                string check = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario AND IdUsuario <> @Id";
                using SqlCommand checkCmd = new SqlCommand(check, conn);
                checkCmd.Parameters.AddWithValue("@Usuario", model.Username.Trim());
                checkCmd.Parameters.AddWithValue("@Id", model.Id);
                if ((int)checkCmd.ExecuteScalar() > 0)
                {
                    ViewBag.Error = "Ya existe otro usuario con ese nombre.";
                    return View(model);
                }

                string query = "UPDATE Usuarios SET Usuario = @Usuario WHERE IdUsuario = @Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Usuario", model.Username.Trim());
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.ExecuteNonQuery();

                TempData["Exito"] = "Usuario actualizado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Eliminar(int id)
        {
            if (!SesionActiva()) return RedirectToAction("Index", "Login");

            try
            {
                using SqlConnection conn = GetConnection();
                conn.Open();
                string query = "DELETE FROM Usuarios WHERE IdUsuario = @Id";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                TempData["Exito"] = "Usuario eliminado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}