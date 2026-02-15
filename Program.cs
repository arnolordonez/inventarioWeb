using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// CONFIGURACIÓN DE CONEXIONES A MySQL
// ==========================================================

// Base de datos de Usuarios
builder.Services.AddDbContext<UsuariosDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ConexionUsuarios"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ConexionUsuarios"))
    )
);

// Base de datos de MovimientoVentas
builder.Services.AddDbContext<MovimientoVentasDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ConexionMovimientoVentas"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ConexionMovimientoVentas"))
    )
);

// ==========================================================
// SERVICIOS MVC + SESIÓN
// ==========================================================

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// NECESARIO PARA USAR HttpContext EN EL LAYOUT
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ==========================================================
// PIPELINE DE LA APLICACIÓN
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar sesiones (REQUERIDO PARA LOGIN)
app.UseSession();

// ==========================================================
// RUTA PRINCIPAL (LOGIN POR DEFECTO)
// ==========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auto}/{action=Login}/{id?}"
);
app.Run();
