using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioWEB.Models
{
    [Table("cliente")]
    public class Cliente
    {
        // ===============================
        // CLAVE PRIMARIA (CÉDULA)
        // ===============================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Cédula")]
        public int ID_Cliente { get; set; }

        // ===============================
        // DATOS PERSONALES
        // ===============================
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required, StringLength(150)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        [Display(Name = "Ciudad / Municipio")]
        public string? CiudadMunicipio { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; }

        // ===============================
        // TIPO CLIENTE
        // ===============================
        [Required, StringLength(50)]
        [Display(Name = "Tipo de cliente")]
        public string TipoCliente { get; set; } = "Minorista";

        [ForeignKey(nameof(TipoCliente))]
        public virtual TipoCliente? TipoClienteNav { get; set; }

        // ===============================
        // OTROS CAMPOS
        // ===============================
        [Column(TypeName = "text")]
        public string? Observaciones { get; set; }

        // VIP por lógica de negocio
        [Column(TypeName = "tinyint(1)")]
        public bool VIP { get; set; } = false;

        // Soft delete
        [Column(TypeName = "tinyint(1)")]
        public bool Activo { get; set; } = true;

        // ===============================
        // SEGURIDAD (NO VIENEN DEL FORM)
        // ===============================
        [BindNever]
        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string HashContrasena { get; set; } = string.Empty;

        [BindNever]
        [ScaffoldColumn(false)]
        [StringLength(255)]
        public string Salt { get; set; } = string.Empty;

        // ===============================
        // RELACIONES
        // ===============================
        public ICollection<Pedido>? Pedidos { get; set; }
    }
}
