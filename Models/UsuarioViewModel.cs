namespace VillexMVC.Models
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class NuevoUsuarioViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}