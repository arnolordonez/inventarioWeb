using Microsoft.EntityFrameworkCore;
using InventarioWEB.Models;

namespace InventarioWEB.Data
{
    public class MovimientoVentasDbContext : DbContext
    {
        public MovimientoVentasDbContext(DbContextOptions<MovimientoVentasDbContext> options)
            : base(options) { }

        // ==========================================================
        // DBSETS
        // ==========================================================

        public DbSet<Abono> Abonos { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Color> Colores { get; set; } = null!;
        public DbSet<DetallePedido> DetallePedidos { get; set; } = null!;
        public DbSet<Genero> Generos { get; set; } = null!;
        public DbSet<MetodoPago> MetodosPago { get; set; } = null!;
        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Referencia> Referencias { get; set; } = null!;
        public DbSet<ReferenciaTela> ReferenciasTelas { get; set; } = null!;
        public DbSet<Talla> Tallas { get; set; } = null!;
        public DbSet<Tela> Telas { get; set; } = null!;
        public DbSet<TipoCliente> TipoClientes { get; set; } = null!;
        public DbSet<PasswordResetCliente> PasswordResetsClientes { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Despacho> Despachos { get; set; } = null!;
        public DbSet<DetalleDespacho> DetalleDespachos { get; set; } = null!;

        // PRODUCCIÓN
        public DbSet<Produccion> Producciones { get; set; } = null!;
        public DbSet<DetalleProduccion> DetalleProducciones { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================================
            // TABLAS
            // ==========================================================

            modelBuilder.Entity<Cliente>().ToTable("cliente");
            modelBuilder.Entity<Pedido>().ToTable("pedido");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");
            modelBuilder.Entity<Abono>().ToTable("abono");
            modelBuilder.Entity<MetodoPago>().ToTable("metodopago");
            modelBuilder.Entity<Producto>().ToTable("productos");
            modelBuilder.Entity<Genero>().ToTable("genero");
            modelBuilder.Entity<Color>().ToTable("colores");
            modelBuilder.Entity<Talla>().ToTable("tallas");
            modelBuilder.Entity<Tela>().ToTable("telas");
            modelBuilder.Entity<Referencia>().ToTable("referencias");
            modelBuilder.Entity<ReferenciaTela>().ToTable("referencias_telas");
            modelBuilder.Entity<TipoCliente>().ToTable("tipocliente");
            modelBuilder.Entity<PasswordResetCliente>().ToTable("passwordresetsclientes");
            modelBuilder.Entity<Usuario>().ToTable("usuario");
            modelBuilder.Entity<Rol>().ToTable("roles");

            modelBuilder.Entity<Produccion>().ToTable("produccion");
            modelBuilder.Entity<DetalleProduccion>().ToTable("detalle_produccion");

            modelBuilder.Entity<Despacho>().ToTable("despacho");
            modelBuilder.Entity<DetalleDespacho>().ToTable("detalle_despacho");

            // ==========================================================
            // ENUMS → STRING (CLAVE)
            // ==========================================================

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
                .HasIndex(p => new { p.Activo, p.FechaProduccion })
                .HasDatabaseName("idx_produccion_fecha");

            // 🔥 CORREGIDO: mismo orden que en BD (CRÍTICO)
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

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoClienteNav)
                .WithMany()
                .HasForeignKey(c => c.TipoCliente)
                .HasPrincipalKey(tc => tc.Nombre)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.ID_Cliente)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.DetallePedidos)
                .HasForeignKey(dp => dp.ID_Pedido)
                .OnDelete(DeleteBehavior.Cascade);
            

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Abono>()
                .HasOne(a => a.Pedido)
                .WithMany(p => p.Abonos)
                .HasForeignKey(a => a.ID_Pedido)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.MetodoPago)
                .WithMany(mp => mp.Pedidos)
                .HasForeignKey(p => p.ID_MetodoPago)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Abono>()
                .HasOne(a => a.MetodoPago)
                .WithMany(mp => mp.Abonos)
                .HasForeignKey(a => a.ID_MetodoPago)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Talla>()
                .HasOne(t => t.Genero)
                .WithMany(g => g.Tallas)
                .HasForeignKey(t => t.ID_Genero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Referencia>()
                .HasOne(r => r.Genero)
                .WithMany(g => g.Referencias)
                .HasForeignKey(r => r.ID_Genero)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReferenciaTela>()
                .HasKey(rt => new { rt.ID_Referencias, rt.ID_Tallas, rt.ID_Telas });

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Talla)
                .WithMany()
                .HasForeignKey(p => p.ID_Tallas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Referencia)
                .WithMany()
                .HasForeignKey(p => p.ID_Referencias)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Tela)
                .WithMany()
                .HasForeignKey(p => p.ID_Telas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.ColorNav)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.ID_Color)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // DESPACHO
            // ============================
                        
            modelBuilder.Entity<Despacho>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Despachos) // 🔥 CORRECCIÓN CLAVE
                .HasForeignKey(d => d.ID_Pedido)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleDespacho>()
                .HasOne(dd => dd.Despacho)
                .WithMany(d => d.Detalles)
                .HasForeignKey(dd => dd.ID_Despacho)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleDespacho>()
                .HasOne(dd => dd.Producto)
                .WithMany()
                .HasForeignKey(dd => dd.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // PRODUCCIÓN
            // ============================

            modelBuilder.Entity<DetalleProduccion>()
                .HasOne(dp => dp.Produccion)
                .WithMany(p => p.Detalles)
                .HasForeignKey(dp => dp.ID_Produccion)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleProduccion>()
                .HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.ID_Producto)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}