using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public int IdRol { get; set; }

        public bool Activo { get; set; }

        // Opcional
        [DataType(DataType.Password)]
        public string? NuevaContrasena { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NuevaContrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmarContrasena { get; set; }
    }
}