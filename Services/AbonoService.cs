using InventarioWEB.Data;
using InventarioWEB.Models;
using InventarioWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Services
{
    public class AbonoService
    {
        private readonly MovimientoVentasDbContext _context;

        public AbonoService(MovimientoVentasDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 🔹 CREAR ABONO (PAGO FINAL)
        // =====================================================
        public void CrearAbono(AbonoVM model)
        {
            // 🔎 Validar usuario
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == model.ID_Usuario);

            if (usuario == null)
                throw new Exception("El usuario no existe.");

            // 🔎 Validar pedido
            var pedido = _context.Pedidos
                .FirstOrDefault(p => p.ID_Pedido == model.ID_Pedido);

            if (pedido == null)
                throw new Exception("El pedido no existe.");

            // 🔒 Validar si ya está pagado
            var existeAbono = _context.Abonos
                .Any(a => a.ID_Pedido == model.ID_Pedido && a.Activo);

            if (existeAbono)
                throw new Exception("Este pedido ya fue cancelado.");

            // 🔎 Validación monto
            if (model.Monto <= 0)
                throw new Exception("El monto debe ser mayor a 0.");

            // 🧱 Crear abono
            var abono = new Abono
            {
                ID_Pedido = model.ID_Pedido,
                Fecha_Abono = DateTime.Now,
                Monto = model.Monto,
                ID_MetodoPago = model.ID_MetodoPago,
                ID_Usuario = model.ID_Usuario,
                Observacion = model.Observacion,
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            _context.Abonos.Add(abono);
            _context.SaveChanges();
        }

        // =====================================================
        // 🔹 LISTAR ABONOS
        // =====================================================
        public List<AbonoVM> ObtenerAbonos()
        {
            var abonos = _context.Abonos
                .Where(a => a.Activo)
                .ToList();

            var usuarios = _context.Usuarios.ToList();

            return abonos.Select(a =>
            {
                var usuario = usuarios
                    .FirstOrDefault(u => u.IdUsuario == a.ID_Usuario);

                return new AbonoVM
                {
                    ID_Abono = a.ID_Abono,
                    ID_Pedido = a.ID_Pedido,
                    Fecha_Abono = a.Fecha_Abono,
                    Monto = a.Monto,
                    ID_MetodoPago = a.ID_MetodoPago,
                    Observacion = a.Observacion,
                    ID_Usuario = a.ID_Usuario,
                    UsuarioNombre = usuario != null
                        ? usuario.Nombres + " " + usuario.Apellidos
                        : "Sin usuario"
                };
            }).ToList();
        }

        // =====================================================
        // 🔹 ELIMINAR (LÓGICO)
        // =====================================================
        public void EliminarAbono(int id)
        {
            var abono = _context.Abonos
                .FirstOrDefault(a => a.ID_Abono == id);

            if (abono == null)
                throw new Exception("Abono no encontrado.");

            abono.Activo = false;

            _context.SaveChanges();
        }
    }
}