
/// <summary>
/// DTO heredado del registro público.
/// Actualmente en desuso.
/// Los usuarios son creados por el Administrador del sistema.
/// </summary>


using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.Models.DTO
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "Debe ingresar una fecha de nacimiento.")]
        [DataType(DataType.Date)]
        public DateTime FechaNac { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }
    }
}
