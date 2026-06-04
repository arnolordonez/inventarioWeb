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
        // 📊 PEDIDO ORIGINAL (DOCENAS)
        // ================================
        public int TotalDocenasPedido { get; set; }

        // ================================
        // 🚚 TOTAL YA DESPACHADO (DOCENAS)
        // ================================
        public int TotalDocenasDespachadas =>
            Tallas.Sum(t => t.CantidadDespachada);

        // ================================
        // ⚖️ TOTAL PENDIENTE (DOCENAS)
        // 🔥 CORREGIDO: ahora sí es real
        // ================================
        public int TotalDocenasPendientes =>
            Math.Max(0, TotalDocenasPedido - TotalDocenasDespachadas);

        // ================================
        // ✍️ INPUT USUARIO
        // ================================
        [MinLength(1, ErrorMessage = "Debe existir al menos una talla")]
        public List<DespachoTallaItemVM> Tallas { get; set; } = new();

        // ================================
        // 🔢 TOTAL INGRESADO (DOCENAS)
        // ================================
        public int TotalDocenasIngresadas =>
            Tallas.Sum(t => t.Cantidad);

        // ================================
        // ✔ VALIDACIONES BÁSICAS (NO NEGOCIO)
        // ================================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Tallas == null || !Tallas.Any())
            {
                yield return new ValidationResult(
                    "Debe ingresar al menos una talla",
                    new[] { nameof(Tallas) });
                yield break;
            }

            // =========================================
            // 🔥 VALIDAR QUE EXISTA AL MENOS UNA CANTIDAD > 0
            // =========================================
            if (!Tallas.Any(t => t.Cantidad > 0))
            {
                yield return new ValidationResult(
                    "Debe ingresar al menos una cantidad mayor a 0",
                    new[] { nameof(Tallas) });
            }

            // =========================================
            // 🚫 NO VALIDAR NEGOCIO AQUÍ
            // =========================================
            // ❌ NO comparar contra pendientes
            // ❌ NO validar stock
            // ❌ NO validar históricos
            //
            // 👉 TODO ESO VA EN EL CONTROLLER (BD = fuente de verdad)
        }
    }

    // ==========================================================
    // ITEM POR TALLA
    // ==========================================================
    public class DespachoTallaItemVM
    {
        // ================================
        // 🔗 IDENTIFICADORES
        // ================================
        [Required]
        public int ID_Detalle { get; set; }

        [Required]
        public int ID_Producto { get; set; }

        // ================================
        // 📦 INFORMACIÓN VISUAL
        // ================================
        public string Talla { get; set; } = string.Empty;

        // ================================
        // 📊 CONTROL (DOCENAS)
        // ================================
        public int CantidadPedida { get; set; }

        public int CantidadDespachada { get; set; }

        // 🔥 NUEVO → PRODUCCIÓN DISPONIBLE REAL
        public int CantidadDisponible { get; set; }

        // 🔥 CALCULADO CORRECTO
        public int CantidadPendiente =>
            Math.Max(0, CantidadPedida - CantidadDespachada);

        // ================================
        // ✍️ INPUT USUARIO (DOCENAS)
        // ================================
        [Range(0, 1000, ErrorMessage = "Cantidad inválida")]
        public int Cantidad { get; set; }
    }
}