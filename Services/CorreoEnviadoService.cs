using InventarioWEB.Data;
using InventarioWEB.Models;

namespace InventarioWEB.Services
{
    // =========================================================
    // 📧 SERVICIO DE AUDITORÍA DE CORREOS
    // =========================================================
    // Responsabilidad:
    // Registrar en base de datos cada intento de envío
    // realizado desde el ERP.
    // =========================================================

    public class CorreoEnviadoService
    {
        private readonly MovimientoVentasDbContext _context;

        public CorreoEnviadoService(
            MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // REGISTRAR ENVÍO
        // =====================================================

        public async Task RegistrarAsync(
            int idPedido,
            string destinatario,
            string usuario,
            string estado,
            string? observaciones = null)
        {
            var correo = new CorreoEnviado
            {
                ID_Pedido = idPedido,
                Destinatario = destinatario,
                FechaEnvio = DateTime.Now,
                Usuario = usuario,
                Estado = estado,
                Observaciones = observaciones
            };

            _context.CorreosEnviados.Add(correo);

            await _context.SaveChangesAsync();
        }
    }
}