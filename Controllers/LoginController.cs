using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VillexMVC.Models;

namespace VillexMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            try
            {
                string connStr = _configuration.GetConnectionString("DefaultConnection")!;
                using SqlConnection conn = new SqlConnection(connStr);
                conn.Open();

                string query = "SELECT COUNT(*) FROM Usuarios WHERE Usuario = @Usuario AND Contrasena = @Contrasena";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Usuario", model.Username);
                cmd.Parameters.AddWithValue("@Contrasena", model.Password);

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    HttpContext.Session.SetString("Usuario", model.Username);
                    return RedirectToAction("Index", "Roles");
                }

                ViewBag.Error = "Usuario o contrasena incorrectos.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexion: " + ex.Message;
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
