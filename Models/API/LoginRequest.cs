namespace InventarioWEB.Models.API
{
    /// <summary>
    /// Modelo que representa la solicitud de inicio de sesión
    /// </summary>
    public class LoginRequest
    {
        public string Correo { get; set; }

        public string Password { get; set; }
    }
}