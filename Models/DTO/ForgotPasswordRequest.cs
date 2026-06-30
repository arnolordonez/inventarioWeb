
// Archivo: Models/DTO/ForgotPasswordRequest.cs
using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Models.DTO
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
