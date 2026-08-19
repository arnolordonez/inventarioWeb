using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarioWEB.Migrations
{
    /// <inheritdoc />
    public partial class InicialTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "colores",
                columns: table => new
                {
                    ID_Color = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colores", x => x.ID_Color);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "genero",
                columns: table => new
                {
                    ID_Genero = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DescripGenero = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genero", x => x.ID_Genero);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "historialinventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentoReferencia = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    IdGenero = table.Column<int>(type: "int", nullable: false),
                    Referencia = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tela = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Talla = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreProducto = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    StockAnterior = table.Column<int>(type: "int", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    DespachoId = table.Column<int>(type: "int", nullable: true),
                    Cliente = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historialinventario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "metodopago",
                columns: table => new
                {
                    ID_MetodoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categoria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metodopago", x => x.ID_MetodoPago);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "produccion",
                columns: table => new
                {
                    ID_Produccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaProduccion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observacion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Usuario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produccion", x => x.ID_Produccion);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreRol = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.IdRol);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "telas",
                columns: table => new
                {
                    ID_Telas = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DescripTela = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telas", x => x.ID_Telas);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipocliente",
                columns: table => new
                {
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipocliente", x => x.Nombre);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "referencias",
                columns: table => new
                {
                    ID_Referencias = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DescripReferencia = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_Genero = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencias", x => x.ID_Referencias);
                    table.ForeignKey(
                        name: "FK_referencias_genero_ID_Genero",
                        column: x => x.ID_Genero,
                        principalTable: "genero",
                        principalColumn: "ID_Genero",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tallas",
                columns: table => new
                {
                    ID_Tallas = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DescripTalla = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_Genero = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tallas", x => x.ID_Tallas);
                    table.ForeignKey(
                        name: "FK_tallas_genero_ID_Genero",
                        column: x => x.ID_Genero,
                        principalTable: "genero",
                        principalColumn: "ID_Genero",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombres = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellidos = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashContrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salt = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaUltimaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_usuario_roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    ID_Cliente = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CiudadMunicipio = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TipoCliente = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VIP = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HashContrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Salt = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.ID_Cliente);
                    table.ForeignKey(
                        name: "FK_cliente_tipocliente_TipoCliente",
                        column: x => x.TipoCliente,
                        principalTable: "tipocliente",
                        principalColumn: "Nombre",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    ID_Producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_Genero = table.Column<int>(type: "int", nullable: false),
                    ID_Referencias = table.Column<int>(type: "int", nullable: false),
                    ID_Tallas = table.Column<int>(type: "int", nullable: false),
                    ID_Telas = table.Column<int>(type: "int", nullable: false),
                    ID_Color = table.Column<int>(type: "int", nullable: false),
                    PrecioCosto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioVTA = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IVA_Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.ID_Producto);
                    table.ForeignKey(
                        name: "FK_productos_colores_ID_Color",
                        column: x => x.ID_Color,
                        principalTable: "colores",
                        principalColumn: "ID_Color",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_genero_ID_Genero",
                        column: x => x.ID_Genero,
                        principalTable: "genero",
                        principalColumn: "ID_Genero",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_productos_referencias_ID_Referencias",
                        column: x => x.ID_Referencias,
                        principalTable: "referencias",
                        principalColumn: "ID_Referencias",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_tallas_ID_Tallas",
                        column: x => x.ID_Tallas,
                        principalTable: "tallas",
                        principalColumn: "ID_Tallas",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_telas_ID_Telas",
                        column: x => x.ID_Telas,
                        principalTable: "telas",
                        principalColumn: "ID_Telas",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "referencias_telas",
                columns: table => new
                {
                    ID_Referencias = table.Column<int>(type: "int", nullable: false),
                    ID_Tallas = table.Column<int>(type: "int", nullable: false),
                    ID_Telas = table.Column<int>(type: "int", nullable: false),
                    ReferenciaID_Referencias = table.Column<int>(type: "int", nullable: false),
                    TallaID_Tallas = table.Column<int>(type: "int", nullable: false),
                    TelaID_Telas = table.Column<int>(type: "int", nullable: false),
                    GeneroID_Genero = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencias_telas", x => new { x.ID_Referencias, x.ID_Tallas, x.ID_Telas });
                    table.ForeignKey(
                        name: "FK_referencias_telas_genero_GeneroID_Genero",
                        column: x => x.GeneroID_Genero,
                        principalTable: "genero",
                        principalColumn: "ID_Genero");
                    table.ForeignKey(
                        name: "FK_referencias_telas_referencias_ReferenciaID_Referencias",
                        column: x => x.ReferenciaID_Referencias,
                        principalTable: "referencias",
                        principalColumn: "ID_Referencias",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referencias_telas_tallas_TallaID_Tallas",
                        column: x => x.TallaID_Tallas,
                        principalTable: "tallas",
                        principalColumn: "ID_Tallas",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_referencias_telas_telas_TelaID_Telas",
                        column: x => x.TelaID_Telas,
                        principalTable: "telas",
                        principalColumn: "ID_Telas",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "passwordresetsclientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Cliente = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiraToken = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Usado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordresetsclientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_passwordresetsclientes_cliente_ID_Cliente",
                        column: x => x.ID_Cliente,
                        principalTable: "cliente",
                        principalColumn: "ID_Cliente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    ID_Pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstadoPago = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_Cliente = table.Column<int>(type: "int", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalIVA = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TipoVenta = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido", x => x.ID_Pedido);
                    table.ForeignKey(
                        name: "FK_pedido_cliente_ID_Cliente",
                        column: x => x.ID_Cliente,
                        principalTable: "cliente",
                        principalColumn: "ID_Cliente",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movimiento_inventario",
                columns: table => new
                {
                    ID_Movimiento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TablaOrigen = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ID_Origen = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Usuario = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_inventario", x => x.ID_Movimiento);
                    table.ForeignKey(
                        name: "FK_movimiento_inventario_productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "abono",
                columns: table => new
                {
                    ID_Abono = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    Fecha_Abono = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ID_MetodoPago = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ID_Usuario = table.Column<int>(type: "int", nullable: true),
                    UsuarioRegistro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NumeroRecibo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RutaRecibo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abono", x => x.ID_Abono);
                    table.ForeignKey(
                        name: "FK_abono_metodopago_ID_MetodoPago",
                        column: x => x.ID_MetodoPago,
                        principalTable: "metodopago",
                        principalColumn: "ID_MetodoPago",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_abono_pedido_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "pedido",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "correo_enviado",
                columns: table => new
                {
                    IdCorreo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    Destinatario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaEnvio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Usuario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correo_enviado", x => x.IdCorreo);
                    table.ForeignKey(
                        name: "FK_correo_enviado_pedido_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "pedido",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "despacho",
                columns: table => new
                {
                    ID_Despacho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacion = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioCreacion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CorreoEnviado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaEnvioCorreo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioEnvioCorreo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorreoDestino = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_despacho", x => x.ID_Despacho);
                    table.ForeignKey(
                        name: "FK_despacho_pedido_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "pedido",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "detalle_pedido",
                columns: table => new
                {
                    ID_Detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Pedido = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IVA_Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IVA_Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_pedido", x => x.ID_Detalle);
                    table.ForeignKey(
                        name: "FK_detalle_pedido_pedido_ID_Pedido",
                        column: x => x.ID_Pedido,
                        principalTable: "pedido",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_pedido_productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EnvioWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdPedido = table.Column<int>(type: "int", nullable: false),
                    IdPedidoNavigationID_Pedido = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdClienteNavigationID_Cliente = table.Column<int>(type: "int", nullable: false),
                    Telefono = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UrlPdf = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaEnvio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvioWhatsApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnvioWhatsApp_cliente_IdClienteNavigationID_Cliente",
                        column: x => x.IdClienteNavigationID_Cliente,
                        principalTable: "cliente",
                        principalColumn: "ID_Cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnvioWhatsApp_pedido_IdPedidoNavigationID_Pedido",
                        column: x => x.IdPedidoNavigationID_Pedido,
                        principalTable: "pedido",
                        principalColumn: "ID_Pedido",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "detalle_produccion",
                columns: table => new
                {
                    ID_Detalle_Produccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Produccion = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    CantidadProducida = table.Column<int>(type: "int", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecioVentaUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SubtotalCosto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    SubtotalVenta = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ID_DetallePedido = table.Column<int>(type: "int", nullable: true),
                    EstadoProduccion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaInicioProduccion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaFinProduccion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ObservacionProduccion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_produccion", x => x.ID_Detalle_Produccion);
                    table.ForeignKey(
                        name: "FK_detalle_produccion_detalle_pedido_ID_DetallePedido",
                        column: x => x.ID_DetallePedido,
                        principalTable: "detalle_pedido",
                        principalColumn: "ID_Detalle");
                    table.ForeignKey(
                        name: "FK_detalle_produccion_produccion_ID_Produccion",
                        column: x => x.ID_Produccion,
                        principalTable: "produccion",
                        principalColumn: "ID_Produccion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_produccion_productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "detalle_despacho",
                columns: table => new
                {
                    ID_DetalleDespacho = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ID_Despacho = table.Column<int>(type: "int", nullable: false),
                    ID_Detalle = table.Column<int>(type: "int", nullable: false),
                    ID_Producto = table.Column<int>(type: "int", nullable: false),
                    ID_Detalle_Produccion = table.Column<int>(type: "int", nullable: true),
                    Cantidad_Despachada = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_despacho", x => x.ID_DetalleDespacho);
                    table.ForeignKey(
                        name: "FK_detalle_despacho_despacho_ID_Despacho",
                        column: x => x.ID_Despacho,
                        principalTable: "despacho",
                        principalColumn: "ID_Despacho",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_despacho_detalle_pedido_ID_Detalle",
                        column: x => x.ID_Detalle,
                        principalTable: "detalle_pedido",
                        principalColumn: "ID_Detalle",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_despacho_detalle_produccion_ID_Detalle_Produccion",
                        column: x => x.ID_Detalle_Produccion,
                        principalTable: "detalle_produccion",
                        principalColumn: "ID_Detalle_Produccion");
                    table.ForeignKey(
                        name: "FK_detalle_despacho_productos_ID_Producto",
                        column: x => x.ID_Producto,
                        principalTable: "productos",
                        principalColumn: "ID_Producto",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_abono_ID_MetodoPago",
                table: "abono",
                column: "ID_MetodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_abono_ID_Pedido",
                table: "abono",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_cliente_TipoCliente",
                table: "cliente",
                column: "TipoCliente");

            migrationBuilder.CreateIndex(
                name: "IX_correo_enviado_ID_Pedido",
                table: "correo_enviado",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_despacho_ID_Pedido",
                table: "despacho",
                column: "ID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_despacho_ID_Despacho_ID_Detalle",
                table: "detalle_despacho",
                columns: new[] { "ID_Despacho", "ID_Detalle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_despacho_ID_Detalle",
                table: "detalle_despacho",
                column: "ID_Detalle");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_despacho_ID_Detalle_Produccion",
                table: "detalle_despacho",
                column: "ID_Detalle_Produccion");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_despacho_ID_Producto",
                table: "detalle_despacho",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_pedido_ID_Pedido_ID_Producto",
                table: "detalle_pedido",
                columns: new[] { "ID_Pedido", "ID_Producto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_detalle_pedido_ID_Producto",
                table: "detalle_pedido",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_produccion_ID_DetallePedido",
                table: "detalle_produccion",
                column: "ID_DetallePedido");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_produccion_ID_Produccion",
                table: "detalle_produccion",
                column: "ID_Produccion");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_produccion_ID_Producto",
                table: "detalle_produccion",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_EnvioWhatsApp_IdClienteNavigationID_Cliente",
                table: "EnvioWhatsApp",
                column: "IdClienteNavigationID_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_EnvioWhatsApp_IdPedidoNavigationID_Pedido",
                table: "EnvioWhatsApp",
                column: "IdPedidoNavigationID_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_inventario_ID_Producto",
                table: "movimiento_inventario",
                column: "ID_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_passwordresetsclientes_ID_Cliente",
                table: "passwordresetsclientes",
                column: "ID_Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_ID_Cliente",
                table: "pedido",
                column: "ID_Cliente");

            migrationBuilder.CreateIndex(
                name: "idx_produccion_fecha",
                table: "produccion",
                columns: new[] { "Activo", "FechaProduccion" });

            migrationBuilder.CreateIndex(
                name: "idx_producto_busqueda_real",
                table: "productos",
                columns: new[] { "ID_Referencias", "ID_Tallas", "ID_Telas", "ID_Color", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_productos_ID_Color",
                table: "productos",
                column: "ID_Color");

            migrationBuilder.CreateIndex(
                name: "IX_productos_ID_Genero",
                table: "productos",
                column: "ID_Genero");

            migrationBuilder.CreateIndex(
                name: "IX_productos_ID_Tallas",
                table: "productos",
                column: "ID_Tallas");

            migrationBuilder.CreateIndex(
                name: "IX_productos_ID_Telas",
                table: "productos",
                column: "ID_Telas");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_ID_Genero",
                table: "referencias",
                column: "ID_Genero");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_telas_GeneroID_Genero",
                table: "referencias_telas",
                column: "GeneroID_Genero");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_telas_ReferenciaID_Referencias",
                table: "referencias_telas",
                column: "ReferenciaID_Referencias");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_telas_TallaID_Tallas",
                table: "referencias_telas",
                column: "TallaID_Tallas");

            migrationBuilder.CreateIndex(
                name: "IX_referencias_telas_TelaID_Telas",
                table: "referencias_telas",
                column: "TelaID_Telas");

            migrationBuilder.CreateIndex(
                name: "IX_tallas_ID_Genero",
                table: "tallas",
                column: "ID_Genero");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_IdRol",
                table: "usuario",
                column: "IdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abono");

            migrationBuilder.DropTable(
                name: "correo_enviado");

            migrationBuilder.DropTable(
                name: "detalle_despacho");

            migrationBuilder.DropTable(
                name: "EnvioWhatsApp");

            migrationBuilder.DropTable(
                name: "historialinventario");

            migrationBuilder.DropTable(
                name: "movimiento_inventario");

            migrationBuilder.DropTable(
                name: "passwordresetsclientes");

            migrationBuilder.DropTable(
                name: "referencias_telas");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "metodopago");

            migrationBuilder.DropTable(
                name: "despacho");

            migrationBuilder.DropTable(
                name: "detalle_produccion");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "detalle_pedido");

            migrationBuilder.DropTable(
                name: "produccion");

            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "colores");

            migrationBuilder.DropTable(
                name: "referencias");

            migrationBuilder.DropTable(
                name: "tallas");

            migrationBuilder.DropTable(
                name: "telas");

            migrationBuilder.DropTable(
                name: "tipocliente");

            migrationBuilder.DropTable(
                name: "genero");
        }
    }
}
