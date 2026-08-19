using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Services;
using InventarioWEB.Configurations;
using System.Globalization;
using InventarioWEB.Services.Interfaces;
using InventarioWEB.Services.Implementations;
using InventarioWEB.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// CULTURA GLOBAL (IMPORTANTE PARA DECIMALES)
// ==========================================================

var culture = new CultureInfo("en-US");

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

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
// REGISTRO DE SERVICIOS DE NEGOCIO
// ==========================================================
// ==========================================================
// SERVICIOS INVENTARIO
// ==========================================================
builder.Services.AddScoped<HistorialInventarioService>();

// ==========================================================
// SERVICIOS VENTAS
// ==========================================================
builder.Services.AddScoped<HistorialVentasService>();
builder.Services.AddScoped<AbonoService>();
builder.Services.AddScoped<ReciboCajaService>();

// ==========================================================
// SERVICIO PDF FACTURA
// ==========================================================
builder.Services.AddScoped<FacturaPdfService>();
// ==========================================================
// SERVICIO DE CORREO ELECTRÓNICO
// ==========================================================
builder.Services.Configure<EmailSettings>(
builder.Configuration.GetSection("Email"));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<CorreoEnviadoService>();

// ==========================================================
// SERVICIOS PRODUCCIÓN
// ==========================================================
builder.Services.AddScoped<ProduccionService>();

// ==========================================================
// MULTI-TENANT
// ==========================================================

builder.Services.AddScoped<ITenantResolver, TenantResolver>();

builder.Services.AddScoped<TenantContext>();

builder.Services.AddScoped<
    ITenantDbContextFactory,
    TenantDbContextRuntimeFactory>();

// ==========================================================
// SERVICIOS MVC
// ==========================================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ==========================================================
// SWAGGER PARA DOCUMENTACIÓN DE API
// ==========================================================

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// ==========================================================
// SESIONES
// ==========================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// NECESARIO PARA USAR HttpContext EN EL LAYOUT
builder.Services.AddHttpContextAccessor();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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

app.UseSession();

app.UseMiddleware<TenantResolverMiddleware>();

app.UseAuthorization();

// ==========================================================
// MAPEO DE CONTROLADORES API
// ==========================================================

//app.MapControllers();

// ==========================================================
// RUTA PRINCIPAL MVC (LOGIN)
// ==========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auto}/{action=Login}/{id?}"
);

app.Run();