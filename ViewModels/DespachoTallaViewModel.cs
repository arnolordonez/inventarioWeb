using System.ComponentModel.DataAnnotations;

namespace InventarioWEB.ViewModels
{
    public class DespachoTallaViewModel : IValidatableObject
    {
        // ================================
        // 🔗 CONTEXTO
        // ================================
        [Required]
        public int ID_Pedido { get; set; }

        // ================================
        // 📦 INFORMACIÓN VISUAL
        // ================================
        public string Producto { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;

        // ================================
        // 📊 PEDIDO ORIGINAL
        // ================================
        public int TotalUnidadesPedido { get; set; }

        public int TotalDocenasPedido => TotalUnidadesPedido / 12;

        // ================================
        // 🚚 DESPACHADO (DINÁMICO ✔)
        // ================================
        public int TotalUnidadesDespachadas =>
            Tallas.Sum(t => t.CantidadDespachada);

        public int TotalDocenasDespachadas => TotalUnidadesDespachadas / 12;

        // ================================
        // ⚖️ SALDO
        // ================================
        public int TotalUnidadesPendientes =>
            Math.Max(0, TotalUnidadesPedido - TotalUnidadesDespachadas);

        public int TotalDocenasPendientes => TotalUnidadesPendientes / 12;

        // ================================
        // ✍️ INPUT USUARIO
        // ================================
        [MinLength(1, ErrorMessage = "Debe ingresar al menos una talla")]
        public List<DespachoTallaItemVM> Tallas { get; set; } = new();

        // ================================
        // 🔢 TOTALES DINÁMICOS
        // ================================
        public int TotalUnidades => Tallas.Sum(t => t.Cantidad);

        public int TotalDocenas => TotalUnidades / 12;

        // ================================
        // ✔ VALIDACIONES GLOBALES
        // ================================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TotalUnidades == 0)
            {
                yield return new ValidationResult(
                    "Debe ingresar al menos una cantidad",
                    new[] { nameof(Tallas) });
            }

            if (TotalUnidades % 12 != 0)
            {
                yield return new ValidationResult(
                    "El despacho debe ser múltiplo de 12 (docenas)");
            }

            if (TotalUnidades > TotalUnidadesPendientes)
            {
                yield return new ValidationResult(
                    "No puede despachar más de lo pendiente");
            }

            foreach (var talla in Tallas)
            {
                if (talla.Cantidad > talla.CantidadPendiente)
                {
                    yield return new ValidationResult(
                        $"La talla {talla.Talla} excede lo pendiente");
                }
            }
        }
    }

    public class DespachoTallaItemVM
    {
        [Required]
        public int ID_Producto { get; set; }

        public string Talla { get; set; } = string.Empty;

        // ================================
        // 📊 CONTROL
        // ================================
        public int CantidadPedida { get; set; }

        public int CantidadDespachada { get; set; }

        public int CantidadPendiente =>
            Math.Max(0, CantidadPedida - CantidadDespachada);

        // ================================
        // ✍️ INPUT USUARIO
        // ================================
        [Range(0, 1000, ErrorMessage = "Cantidad inválida")]
        public int Cantidad { get; set; }
    }
}