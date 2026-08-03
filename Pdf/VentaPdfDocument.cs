using InventarioWEB.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventarioWEB.Pdf
{
    public class VentaPdfDocument : IDocument
    {
        private readonly VentaDetalleVM _model;

        // ==========================================================
        // CONSTRUCTOR
        // ==========================================================
        public VentaPdfDocument(VentaDetalleVM model)
        {
            _model = model;
        }

        // ==========================================================
        // METADATOS DEL DOCUMENTO
        // ==========================================================
        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = $"Reporte Venta {_model.ID_Pedido}",
                Author = "ERP INVENTARIOWEB",
                Subject = "Reporte Comercial",
                Creator = "ERP INVENTARIOWEB",
                Producer = "QuestPDF",
                Keywords = "ERP, Venta, Pedido, Reporte, Inventario"
            };
        }

        // ==========================================================
        // DOCUMENTO
        // ==========================================================
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);

                page.Margin(25);

                page.PageColor(Colors.White);

                page.DefaultTextStyle(style =>
                    style.FontSize(10)
                         .FontColor(Colors.Grey.Darken4));

                // ==================================================
                // ENCABEZADO
                // ==================================================
                page.Header()
                    .Element(ComposeHeader);

                // ==================================================
                // CONTENIDO
                // ==================================================
                page.Content()
                    .PaddingVertical(15)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        column.Item().Element(ComposeInformacionGeneral);

                        column.Item().Element(ComposeEstadoVenta);

                        column.Item().Element(ComposeResumenFinanciero);

                        column.Item().Element(ComposeInformacionTributaria);

                        column.Item().Element(ComposeProductos);

                        // =============================================
                        // Totales + Historial
                        // =============================================
                        column.Item()
                            .ShowEntire()
                            .Column(c =>
                            {
                                c.Spacing(15);

                                c.Item().Element(ComposeTotales);

                                c.Item().Element(ComposeHistorialPagos);
                            });

                        // =============================================
                        // Observaciones + Firmas
                        // =============================================
                        column.Item()
                            .ShowEntire()
                            .Column(c =>
                            {
                                c.Spacing(15);

                                c.Item().Element(ComposeObservaciones);

                                c.Item().Element(ComposeFirmas);
                            });
                    });

                // ==================================================
                // PIE DE PÁGINA
                // ==================================================
                page.Footer()
                    .Element(ComposeFooter);
            });
        }
        // ==========================================================
        // ENCABEZADO
        // ==========================================================
        private void ComposeHeader(IContainer container)
        {
            var logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "img",
                "Logo.jpg");

            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // --------------------------------------------------
                    // LOGO
                    // --------------------------------------------------
                    row.ConstantItem(80)
                        .Height(80)
                        .Image(logoPath);

                    // --------------------------------------------------
                    // INFORMACIÓN DEL DOCUMENTO
                    // --------------------------------------------------
                    row.RelativeItem()
                        .PaddingLeft(15)
                        .Column(info =>
                        {
                            info.Item()
                                .Text("ERP INVENTARIOWEB")
                                .Bold()
                                .FontSize(22)
                                .FontColor(Colors.Blue.Darken3);

                            info.Item()
                                .Text(_model.TituloReporte)
                                .Bold()
                                .FontSize(15);

                            info.Item()
                                .Text($"Factura: {_model.NumeroFactura}")
                                .FontSize(10);

                            info.Item()
                                .Text($"Pedido: {_model.ID_Pedido}")
                                .FontSize(10);

                            info.Item()
                                .Text("Documento generado automáticamente")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                });

                column.Item()
                    .PaddingTop(10)
                    .LineHorizontal(1);
            });
        }

        // ==========================================================
        // INFORMACIÓN GENERAL
        // ==========================================================
        private void ComposeInformacionGeneral(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("INFORMACIÓN GENERAL")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        AddRow(
                            table,
                            "Factura",
                            _model.NumeroFactura,
                            "Pedido",
                            _model.ID_Pedido.ToString());

                        AddRow(
                            table,
                            "Cliente",
                            _model.Cliente,
                            "Fecha Venta",
                            _model.Fecha.ToString("dd/MM/yyyy"));

                        AddRow(
                            table,
                            "Fecha Reporte",
                            _model.FechaReporte.ToString("dd/MM/yyyy HH:mm"),
                            "Tipo Venta",
                            _model.TipoVenta);
                    });
            });
        }

        // ==========================================================
        // ESTADO DE LA VENTA
        // ==========================================================
        private void ComposeEstadoVenta(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("ESTADO DE LA VENTA")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Primera fila
                        AddRow(
                            table,
                            "Pedido",
                            (_model.EstadoPedido ?? string.Empty).ToUpper(),
                            "Pago",
                            (_model.EstadoPago ?? string.Empty).ToUpper());

                        // Segunda fila
                        AddRow(
                            table,
                            "Despacho",
                            (_model.EstadoDespacho ?? string.Empty).ToUpper(),
                            "Tipo",
                            (_model.TipoDespacho ?? string.Empty).ToUpper());
                    });
            });
        }
        
        // ==========================================================
        // RESUMEN FINANCIERO
        // ==========================================================
        private void ComposeResumenFinanciero(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("RESUMEN FINANCIERO")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            HeaderTableCell(header.Cell(), "Total Venta");
                            HeaderTableCell(header.Cell(), "Total Abonado");
                            HeaderTableCell(header.Cell(), "Saldo");
                            HeaderTableCell(header.Cell(), "Productos");
                            HeaderTableCell(header.Cell(), "Unidades");
                        });

                        table.Cell().AlignRight().Text(_model.TotalVenta.ToString("C0"));
                        table.Cell().AlignRight().Text(_model.TotalAbonado.ToString("C0"));
                        table.Cell().AlignRight().Text(_model.Saldo.ToString("C0"));
                        table.Cell().AlignCenter().Text(_model.TotalProductos.ToString());
                        table.Cell().AlignCenter().Text(_model.TotalUnidades.ToString());
                    });
            });
        }

        // ==========================================================
        // PIE DE PÁGINA
        // ==========================================================
        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1);

                column.Item().PaddingTop(5);

                column.Item().Row(row =>
                {
                    row.RelativeItem()
                         .Text(text =>
                     {
                         text.DefaultTextStyle(x => x.FontSize(9));

                         text.Span("ERP INVENTARIOWEB").Bold();

                         text.Span("   ");

                         text.Span($"Documento generado el {_model.FechaReporte:dd/MM/yyyy HH:mm}");
                     });
                        
                          row.ConstantItem(120)
                         .AlignRight()
                         .Text(text =>
                         {
                             text.DefaultTextStyle(x => x.FontSize(9));

                             text.Span("Página ");

                             text.CurrentPageNumber();

                             text.Span(" de ");

                             text.TotalPages();
                         });
                });
            });
        }

        // ==========================================================
        // MÉTODOS AUXILIARES
        // ==========================================================
        private static void AddRow(
            TableDescriptor table,
            string t1,
            string v1,
            string t2,
            string v2)
        {
            table.Cell().PaddingBottom(6).Text(t1).Bold();
            table.Cell().PaddingBottom(6).Text(v1);

            table.Cell().PaddingBottom(6).Text(t2).Bold();
            table.Cell().PaddingBottom(6).Text(v2);
        }

        private static void AddRow(
            TableDescriptor table,
            string t1,
            string v1,
            string t2,
            string v2,
            string t3,
            string v3,
            string t4,
            string v4)
        {
            table.Cell().PaddingBottom(6).Text(t1).Bold();
            table.Cell().PaddingBottom(6).Text(v1);

            table.Cell().PaddingBottom(6).Text(t2).Bold();
            table.Cell().PaddingBottom(6).Text(v2);

            table.Cell().PaddingBottom(6).Text(t3).Bold();
            table.Cell().PaddingBottom(6).Text(v3);

            table.Cell().PaddingBottom(6).Text(t4).Bold();
            table.Cell().PaddingBottom(6).Text(v4);
        }


        // ==========================================================
        // INFORMACIÓN TRIBUTARIA
        // ==========================================================
        private void ComposeInformacionTributaria(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("INFORMACIÓN TRIBUTARIA")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Concepto
                            columns.RelativeColumn(2); // Valor
                        });

                        AgregarFila(table, "Base Gravable", _model.Total);
                        AgregarFila(table, "IVA", _model.TotalIVA);
                        AgregarFila(table, "Total Facturado", _model.TotalVenta, true);
                    });
            });
        }

        // ==========================================================
        // PRODUCTOS VENDIDOS
        // ==========================================================
        private void ComposeProductos(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("PRODUCTOS VENDIDOS")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(10)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            // Aproximadamente el 50 % del ancho para el producto
                            columns.RelativeColumn(8);     // Producto
                            columns.RelativeColumn(2);     // Talla
                            columns.RelativeColumn(2);     // Color
                            columns.ConstantColumn(60);    // Cantidad
                            columns.ConstantColumn(95);    // Precio
                            columns.ConstantColumn(110);   // Subtotal
                        });

                        // ======================================================
                        // ENCABEZADO
                        // ======================================================

                        table.Header(header =>
                        {
                            HeaderTableCell(header.Cell(), "Producto");
                            HeaderTableCell(header.Cell(), "Talla");
                            HeaderTableCell(header.Cell(), "Color");
                            HeaderTableCell(header.Cell(), "Cant.");
                            HeaderTableCell(header.Cell(), "Precio");
                            HeaderTableCell(header.Cell(), "Subtotal");
                        });

                        // ======================================================
                        // DETALLE
                        // ======================================================

                        var fila = 0;

                        foreach (var producto in _model.Productos)
                        {
                            var fondo = fila % 2 == 0
                                ? Colors.White
                                : Colors.Grey.Lighten4;

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .Padding(5)
                                .Text(producto.Producto);

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .AlignCenter()
                                .Padding(5)
                                .Text(producto.Talla);

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .AlignCenter()
                                .Padding(5)
                                .Text(producto.Color);

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .AlignCenter()
                                .Padding(5)
                                .Text(producto.Cantidad.ToString());

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .AlignRight()
                                .Padding(5)
                                .Text(Moneda(producto.PrecioVenta));

                            table.Cell()
                                .Background(fondo)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .AlignRight()
                                .Padding(5)
                                .Text(Moneda(producto.Subtotal));

                            fila++;
                        }
                    });
            });
        }


        // ==========================================================
        // TOTALES
        // ==========================================================
        private void ComposeTotales(IContainer container)
        {
            container.AlignRight()
                .Width(300)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(120);
                    });

                    AgregarFila(table, "Base Gravable", _model.Total);
                    AgregarFila(table, "IVA", _model.TotalIVA);

                    AgregarFila(table, "Total Facturado", _model.TotalVenta, true);

                    AgregarFila(table, "Total Abonado", _model.TotalAbonado);

                    AgregarFila(table, "Saldo Pendiente", _model.Saldo, false, true);
                });
        }

        private static void AgregarFila(
            TableDescriptor table,
            string titulo,
            decimal valor,
            bool destacar = false,
            bool saldo = false)
        {
            var fondo = destacar
                ? Colors.Blue.Lighten5
                : Colors.White;

            var estilo = destacar || saldo
                ? TextStyle.Default.Bold().FontSize(11)
                : TextStyle.Default.FontSize(10);

            table.Cell()
                .Background(fondo)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingHorizontal(8)
                .Text(titulo)
                .Style(estilo);

            table.Cell()
                .Background(fondo)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingHorizontal(8)
                .AlignRight()
                .Text(Moneda(valor))
                .Style(estilo);
        }

        // ==========================================================
        // HISTORIAL DE PAGOS
        // ==========================================================
        private void ComposeHistorialPagos(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("HISTORIAL DE PAGOS")
                    .Bold();

                if (_model.TieneAbonos && _model.Abonos.Any())
                {
                    column.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .Padding(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);     // Fecha
                                columns.ConstantColumn(95);     // Método
                                columns.RelativeColumn();       // Recibo
                                columns.ConstantColumn(110);    // Monto
                            });

                            table.Header(header =>
                            {
                                HeaderTableCell(header.Cell(), "Fecha");
                                HeaderTableCell(header.Cell(), "Método");
                                HeaderTableCell(header.Cell(), "Recibo");
                                HeaderTableCell(header.Cell(), "Monto");
                            });

                            var fila = 0;

                            foreach (var abono in _model.Abonos.OrderBy(a => a.Fecha_Abono))
                            {
                                var fondo = fila % 2 == 0
                                    ? "#FFFFFF"
                                    : "#F7F7F7";

                                BodyCell(
                                    table,
                                    abono.Fecha_Abono.ToString("dd/MM/yyyy"),
                                    fondo,
                                    centrado: true);

                                BodyCell(
                                    table,
                                    abono.MetodoPago,
                                    fondo);

                                BodyCell(
                                    table,
                                    abono.NumeroRecibo,
                                    fondo);

                                BodyCell(
                                    table,
                                    Moneda(abono.Monto),
                                    fondo,
                                    derecha: true);

                                fila++;
                            }
                        });

                    column.Item()
                        .PaddingTop(10)
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Total abonado: ").Bold();

                            text.Span(Moneda(_model.TotalAbonado))
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                        });
                }
                else
                {
                    column.Item()
                        .Padding(15)
                        .AlignCenter()
                        .Text("No existen abonos registrados para esta venta.")
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                }
            });
        }

        // ==========================================================
        // OBSERVACIONES
        // ==========================================================
        private void ComposeObservaciones(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Background(Colors.Grey.Lighten2)
                    .Padding(6)
                    .Text("OBSERVACIONES")
                    .Bold();

                column.Item()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten1)
                    .Padding(12)
                    .Text(text =>
                    {
                        text.Span("Este documento corresponde al estado comercial de la venta registrada en ");

                        text.Span("ERP INVENTARIOWEB")
                            .Bold();

                        text.Span(". La información presentada refleja los datos almacenados en la base de datos al momento de generar este reporte y constituye un documento informativo para consulta administrativa.");
                    });
            });
        }


        // ==========================================================
        // FIRMAS
        // ==========================================================
        private void ComposeFirmas(IContainer container)
        {
            container
                .PaddingTop(40)
                .Row(row =>
                {
                    row.RelativeItem()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .Width(180)
                                .LineHorizontal(1);

                            column.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text("Responsable")
                                .Bold();
                        });

                    row.RelativeItem()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .Width(180)
                                .LineHorizontal(1);

                            column.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text("Cliente")
                                .Bold();
                        });
                });
        }


        // ==========================================================
        // CELDA ENCABEZADO TABLAS
        // ==========================================================
        private static void HeaderTableCell(
            QuestPDF.Elements.Table.ITableCellContainer cell,
            string texto)
        {
            cell
                .Background("#E3F2FD")
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .AlignCenter()
                .Text(texto)
                .Bold()
                .FontSize(9);
        }

        // ==========================================================
        // CELDA CUERPO TABLAS
        // ==========================================================
        private static void BodyCell(
            TableDescriptor table,
            string texto,
            string fondo = "#FFFFFF",
            bool centrado = false,
            bool derecha = false)
        {
            IContainer container = table.Cell()
                .Background(fondo)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(5);

            if (centrado)
                container = container.AlignCenter();

            if (derecha)
                container = container.AlignRight();

            container
                .Text(texto ?? string.Empty)
                .FontSize(9);
        }

        // ==========================================================
        // FORMATO MONEDA
        // ==========================================================
        private static string Moneda(decimal valor)
        {
            return valor.ToString("C0");
        }

    }
}