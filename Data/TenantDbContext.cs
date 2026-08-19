using InventarioWEB.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioWEB.Data
{
    /// <summary>
    /// Contexto de base de datos operativa de un Tenant.
    /// Cada empresa posee una base de datos operativa independiente.
    /// La cadena de conexión se determina dinámicamente
    /// para cada solicitud HTTP.
    /// </summary>
    public class TenantDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia del contexto del Tenant.
        /// </summary>
        public TenantDbContext(
        DbContextOptions<TenantDbContext> options)
        : base(options)
        {
        }
        #region DbSets

        public DbSet<Abono> Abonos => Set<Abono>();
        public DbSet<Rol> Roles => Set<Rol>();

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        public DbSet<Cliente> Clientes => Set<Cliente>();

        public DbSet<Color> Colores => Set<Color>();

        public DbSet<DetallePedido> DetallePedidos => Set<DetallePedido>();

        public DbSet<Genero> Generos => Set<Genero>();

        public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();

        public DbSet<Pedido> Pedidos => Set<Pedido>();

        public DbSet<Producto> Productos => Set<Producto>();

        public DbSet<Referencia> Referencias => Set<Referencia>();

        public DbSet<ReferenciaTela> ReferenciasTelas => Set<ReferenciaTela>();

        public DbSet<Talla> Tallas => Set<Talla>();

        public DbSet<Tela> Telas => Set<Tela>();

        public DbSet<TipoCliente> TipoClientes => Set<TipoCliente>();

        public DbSet<PasswordResetCliente> PasswordResetsClientes =>
            Set<PasswordResetCliente>();

      

        public DbSet<Despacho> Despachos => Set<Despacho>();

        public DbSet<DetalleDespacho> DetalleDespachos =>
            Set<DetalleDespacho>();

        public DbSet<MovimientoInventario> MovimientoInventarios =>
            Set<MovimientoInventario>();

        public DbSet<EnvioWhatsApp> EnvioWhatsApp =>
            Set<EnvioWhatsApp>();

        public DbSet<HistorialInventario> HistorialInventario =>
            Set<HistorialInventario>();

        public DbSet<CorreoEnviado> CorreosEnviados =>
            Set<CorreoEnviado>();

        public DbSet<Produccion> Producciones =>
            Set<Produccion>();

        public DbSet<DetalleProduccion> DetalleProducciones =>
            Set<DetalleProduccion>();

        #endregion

        /// <summary>
        /// Configura el modelo de datos operativo del Tenant.
        /// La configuración corresponde al ERP InventarioWEB existente.
        /// </summary>
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================================
            // TABLAS
            // ==========================================================

            modelBuilder.Entity<Cliente>()
                .ToTable("cliente");

            modelBuilder.Entity<Pedido>()
                .ToTable("pedido");

            modelBuilder.Entity<DetallePedido>()
                .ToTable("detalle_pedido");

            modelBuilder.Entity<Abono>()
                .ToTable("abono");

            modelBuilder.Entity<MetodoPago>()
                .ToTable("metodopago");

            modelBuilder.Entity<Producto>()
                .ToTable("productos");

            modelBuilder.Entity<Genero>()
                .ToTable("genero");

            modelBuilder.Entity<Color>()
                .ToTable("colores");

            modelBuilder.Entity<Talla>()
                .ToTable("tallas");

            modelBuilder.Entity<Tela>()
                .ToTable("telas");

            modelBuilder.Entity<Referencia>()
                .ToTable("referencias");

            modelBuilder.Entity<ReferenciaTela>()
                .ToTable("referencias_telas");

            modelBuilder.Entity<TipoCliente>()
                .ToTable("tipocliente");

            modelBuilder.Entity<PasswordResetCliente>()
                .ToTable("passwordresetsclientes");

            
            modelBuilder.Entity<Produccion>()
                .ToTable("produccion");

            modelBuilder.Entity<DetalleProduccion>()
                .ToTable("detalle_produccion");

            modelBuilder.Entity<Despacho>()
                .ToTable("despacho");

            modelBuilder.Entity<DetalleDespacho>()
                .ToTable("detalle_despacho");

            modelBuilder.Entity<MovimientoInventario>()
                .ToTable("movimiento_inventario");

            // ==========================================================
            // PROPIEDADES
            // ==========================================================

            modelBuilder.Entity<DetallePedido>()
                .Ignore("Cantidad_Despachada");

            modelBuilder.Entity<Despacho>()
                .Property(d => d.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Despacho>()
                .Property(d => d.Estado)
                .HasConversion<string>();

            // ==========================================================
            // ÍNDICES
            // ==========================================================

            modelBuilder.Entity<Produccion>()
                .HasIndex(p => new
                {
                    p.Activo,
                    p.FechaProduccion
                })
                .HasDatabaseName("idx_produccion_fecha");

            modelBuilder.Entity<Producto>()
                .HasIndex(p => new
                {
                    p.ID_Referencias,
                    p.ID_Tallas,
                    p.ID_Telas,
                    p.ID_Color,
                    p.Activo
                })
                .HasDatabaseName("idx_producto_busqueda_real");

            // ==========================================================
            // RELACIONES
            // ==========================================================

            // Cliente -> TipoCliente

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoClienteNav)
                .WithMany()
                .HasForeignKey(c => c.TipoCliente)
                .HasPrincipalKey(tc => tc.Nombre)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido -> Cliente

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            // DetallePedido -> Pedido

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.DetallePedidos)
                .HasForeignKey(dp => dp.ID_Pedido)
                .OnDelete(DeleteBehavior.Cascade);

            // DetallePedido -> Producto

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            // Abono -> Pedido

            modelBuilder.Entity<Abono>()
                .HasOne(a => a.Pedido)
                .WithMany(p => p.Abonos)
                .HasForeignKey(a => a.ID_Pedido)
                .OnDelete(DeleteBehavior.Cascade);

            // Abono -> MetodoPago

            modelBuilder.Entity<Abono>()
                .HasOne(a => a.MetodoPago)
                .WithMany(mp => mp.Abonos)
                .HasForeignKey(a => a.ID_MetodoPago)
                .OnDelete(DeleteBehavior.Restrict);

            // Talla -> Genero

            modelBuilder.Entity<Talla>()
                .HasOne(t => t.Genero)
                .WithMany(g => g.Tallas)
                .HasForeignKey(t => t.ID_Genero)
                .OnDelete(DeleteBehavior.Restrict);

            // Referencia -> Genero

            modelBuilder.Entity<Referencia>()
                .HasOne(r => r.Genero)
                .WithMany(g => g.Referencias)
                .HasForeignKey(r => r.ID_Genero)
                .OnDelete(DeleteBehavior.Restrict);

            // ReferenciaTela -> clave compuesta

            modelBuilder.Entity<ReferenciaTela>()
                .HasKey(rt => new
                {
                    rt.ID_Referencias,
                    rt.ID_Tallas,
                    rt.ID_Telas
                });

           
            // Producto -> Talla

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Talla)
                .WithMany()
                .HasForeignKey(p => p.ID_Tallas)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto -> Referencia

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Referencia)
                .WithMany()
                .HasForeignKey(p => p.ID_Referencias)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto -> Tela

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Tela)
                .WithMany()
                .HasForeignKey(p => p.ID_Telas)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto -> Color

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.ColorNav)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.ID_Color)
                .OnDelete(DeleteBehavior.Restrict);

            // MovimientoInventario -> Producto

            modelBuilder.Entity<MovimientoInventario>()
                .HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================================
            // DESPACHOS
            // ==========================================================

            // Despacho -> Pedido

            modelBuilder.Entity<Despacho>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Despachos)
                .HasForeignKey(d => d.ID_Pedido)
                .OnDelete(DeleteBehavior.Restrict);

            // DetalleDespacho -> Despacho

            modelBuilder.Entity<DetalleDespacho>()
                .HasOne(dd => dd.Despacho)
                .WithMany(d => d.Detalles)
                .HasForeignKey(dd => dd.ID_Despacho)
                .OnDelete(DeleteBehavior.Cascade);

            // DetalleDespacho -> Producto

            modelBuilder.Entity<DetalleDespacho>()
                .HasOne(dd => dd.Producto)
                .WithMany()
                .HasForeignKey(dd => dd.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================================
            // PRODUCCIÓN
            // ==========================================================

            // DetalleProduccion -> Produccion

            modelBuilder.Entity<DetalleProduccion>()
                .HasOne(dp => dp.Produccion)
                .WithMany(p => p.Detalles)
                .HasForeignKey(dp => dp.ID_Produccion)
                .OnDelete(DeleteBehavior.Cascade);

            // DetalleProduccion -> Producto

            modelBuilder.Entity<DetalleProduccion>()
                .HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================================
            // PROPIEDADES IGNORADAS
            // ==========================================================

            modelBuilder.Entity<Pedido>()
                .Ignore("MetodoPagoID_MetodoPago");
        }
    }
}