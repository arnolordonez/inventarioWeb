using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventarioWEB.Data
{
    /// <summary>
    /// Fábrica utilizada por Entity Framework Core para crear
    /// una instancia de TenantDbContext durante operaciones
    /// de diseño como Add-Migration y Update-Database.
    ///
    /// Esta fábrica no participa en la resolución dinámica
    /// de los Tenants durante la ejecución de la aplicación.
    /// </summary>
    public class TenantDbContextFactory
        : IDesignTimeDbContextFactory<TenantDbContext>
    {
        /// <summary>
        /// Crea una instancia de TenantDbContext para las
        /// herramientas de Entity Framework Core.
        /// </summary>
        public TenantDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                "Server=localhost;Port=3306;" +
                "Database=tenant_template;" +
                "User=root;Password=123!;";

            var optionsBuilder =
                new DbContextOptionsBuilder<TenantDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                options =>
                {
                    options.MigrationsAssembly(
                        typeof(TenantDbContext).Assembly.FullName);
                });

            return new TenantDbContext(
                optionsBuilder.Options);
        }
    }
}