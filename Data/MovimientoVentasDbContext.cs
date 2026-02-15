using Microsoft.EntityFrameworkCore;
using InventarioWEB.Models;

namespace InventarioWEB.Data
{
    /// <summary>
    /// Contexto principal de base de datos de la aplicación InventarioWEB.
    /// Gestiona el acceso a datos mediante Entity Framework Core y define:
    /// 
    /// • Mapeo de entidades a tablas físicas.
    /// • Configuración de relaciones entre entidades.
    /// • Restricciones de integridad referencial.
    /// • Comportamientos de eliminación (Cascade / Restrict).
    /// 
    /// Representa la unidad de trabajo central del sistema.
    /// </summary>
    public class MovimientoVentasDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe las opciones de configuración del contexto.
        /// </summary>
        /// <param name="options">
        /// Opciones de configuración del DbContext, incluyendo cadena de conexión
        /// y proveedor de base de datos.
        /// </param>
        public MovimientoVentasDbContext(DbContextOptions<MovimientoVentasDbContext> options)
            : base(options) { }

        // ==========================================================
        // DBSETS (Representación de Tablas)
        // ==========================================================

        /// <summary>Tabla de abonos realizados sobre pedidos.</summary>
        public DbSet<Abono> Abonos { get; set; } = null!;

        /// <summary>Tabla de clientes registrados en el sistema.</summary>
        public DbSet<Cliente> Clientes { get; set; } = null!;

        /// <summary>Tabla de colores disponibles para productos.</summary>
        public DbSet<Color> Colores { get; set; } = null!;

        /// <summary>Tabla de detalles asociados a un pedido.</summary>
        public DbSet<DetallePedido> DetallePedidos { get; set; } = null!;

        /// <summary>Tabla de géneros (masculino, femenino, etc.).</summary>
        public DbSet<Genero> Generos { get; set; } = null!;

        /// <summary>Tabla de métodos de pago.</summary>
        public DbSet<MetodoPago> MetodosPago { get; set; } = null!;

        /// <summary>Tabla principal de pedidos realizados por clientes.</summary>
        public DbSet<Pedido> Pedidos { get; set; } = null!;

        /// <summary>Tabla de productos finales disponibles para venta.</summary>
        public DbSet<Producto> Productos { get; set; } = null!;

        /// <summary>Tabla de referencias base de productos.</summary>
        public DbSet<Referencia> Referencias { get; set; } = null!;

        /// <summary>Tabla técnica de combinación entre referencia, talla y tela.</summary>
        public DbSet<ReferenciaTela> ReferenciasTelas { get; set; } = null!;

        /// <summary>Tabla de tallas disponibles.</summary>
        public DbSet<Talla> Tallas { get; set; } = null!;

        /// <summary>Tabla de tipos de tela disponibles.</summary>
        public DbSet<Tela> Telas { get; set; } = null!;

        /// <summary>Tabla de tipos de cliente (Minorista, Mayorista, etc.).</summary>
        public DbSet<TipoCliente> TipoClientes { get; set; } = null!;

        /// <summary>Tabla para gestión de recuperación de contraseña de clientes.</summary>
        public DbSet<PasswordResetCliente> PasswordResetsClientes { get; set; } = null!;

        /// <summary>Tabla de usuarios administrativos del sistema.</summary>
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        /// <summary>Tabla de roles del sistema (Admin, Vendedor, etc.).</summary>
        public DbSet<Rol> Roles { get; set; } = null!;


        /// <summary>
        /// Configura el modelo de datos mediante Fluent API.
        /// Define nombres físicos de tablas, claves primarias,
        /// relaciones y comportamientos de eliminación.
        /// </summary>
        /// <param name="modelBuilder">
        /// Constructor del modelo utilizado para configurar entidades y relaciones.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================================
            // MAPEO EXPLÍCITO DE TABLAS
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

            // ==========================================================
            // RELACIONES PRINCIPALES DEL SISTEMA
            // ==========================================================

            // Cliente ↔ TipoCliente
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.TipoClienteNav)
                .WithMany()
                .HasForeignKey(c => c.TipoCliente)
                .HasPrincipalKey(tc => tc.Nombre)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente ↔ Pedido (1:N)
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

            // Pedidos / Abonos / MétodoPago
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

            // Género
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

            // ReferenciaTela (Clave compuesta técnica)
            modelBuilder.Entity<ReferenciaTela>()
                .HasKey(rt => new
                {
                    rt.ID_Referencias,
                    rt.ID_Tallas,
                    rt.ID_Telas
                });

            modelBuilder.Entity<ReferenciaTela>()
                .HasOne(rt => rt.Referencia)
                .WithMany()
                .HasForeignKey(rt => rt.ID_Referencias)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReferenciaTela>()
                .HasOne(rt => rt.Talla)
                .WithMany()
                .HasForeignKey(rt => rt.ID_Tallas)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReferenciaTela>()
                .HasOne(rt => rt.Tela)
                .WithMany()
                .HasForeignKey(rt => rt.ID_Telas)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario ↔ Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto (Entidad final)
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
        }
    }
}
