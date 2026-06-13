using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    /// <summary>
    /// ViewModel utilizado para la creación de usuarios
    /// desde el módulo de Administración.
    /// </summary>
    public class UsuarioCreateViewModel
    {
        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Los nombres deben tener entre 2 y 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Los apellidos deben tener entre 2 y 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(
            150,
            ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar un rol.")]
        public int IdRol { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Contrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}