namespace InventarioWEB.Models.API
{
    /// <summary>
    /// Modelo utilizado para registrar un nuevo usuario
    /// </summary>
    public class RegisterRequest
    {
        public string Nombres { get; set; }

        public string Apellidos { get; set; }

        public string Correo { get; set; }

        public string Password { get; set; }

        public int IdRol { get; set; }
    }
}