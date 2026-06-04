using Microsoft.EntityFrameworkCore;
using InventarioWEB.Data;
using InventarioWEB.Services;

using InventarioWEB.Mongo;
using InventarioWEB.Mongo.Context;
using InventarioWEB.Mongo.Services;
using Microsoft.Extensions.Options;
using System.Globalization;

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

builder.Services.AddScoped<ProduccionService>();
builder.Services.AddScoped<AbonoService>();// Línea que se incluyó para registrar el servicio de AbonoService
builder.Services.AddScoped<ReciboCajaService>();

// ==========================================================
// CONFIGURACIÓN MONGODB
// ==========================================================

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoDbContext(settings);
});

builder.Services.AddScoped<ColorService>();

// ==========================================================
// SERVICIOS MVC + API
// ==========================================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

  //builder.Services.AddControllersWithViews(); // MVC
  //builder.Services.AddControllers();          // API

// ==========================================================
// SWAGGER PARA DOCUMENTACIÓN DE API
// ==========================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================================
// SESIONES
// ==========================================================

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar sesiones (REQUERIDO PARA LOGIN)
app.UseSession();

app.UseAuthorization();

// ==========================================================
// MAPEO DE CONTROLADORES API
// ==========================================================

app.MapControllers();

// ==========================================================
// RUTA PRINCIPAL MVC (LOGIN)
// ==========================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auto}/{action=Login}/{id?}"
);

app.Run();