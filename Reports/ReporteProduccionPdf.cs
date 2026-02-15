using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InventarioWEB.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventarioWEB.Reports
{
    public class ReporteProduccionPdf : IDocument
    {
        private readonly List<Producto> _productos;

        public ReporteProduccionPdf(List<Producto> productos)
        {
            _productos = productos;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(Header);
                page.Content().Element(Content);
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Confesiones Indomable · ");
                    txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        }

        void Header(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Text("REPORTE DE PRODUCCIÓN")
                    .FontSize(16)
                    .Bold();

                row.ConstantColumn(200).AlignRight()
                    .Text($"Fecha impresión: {DateTime.Now:dd/MM/yyyy}");
            });
        }

        void Content(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60); // Código
                    columns.RelativeColumn();   // Referencia
                    columns.RelativeColumn();   // Tela
                    columns.ConstantColumn(50); // Talla
                    columns.RelativeColumn();   // Color
                    columns.ConstantColumn(60); // Stock
                });

                table.Header(header =>
                {
                    header.Cell().Text("Código").Bold();
                    header.Cell().Text("Referencia").Bold();
                    header.Cell().Text("Tela").Bold();
                    header.Cell().Text("Talla").Bold();
                    header.Cell().Text("Color").Bold();
                    header.Cell().Text("Stock").Bold();
                });

                foreach (var p in _productos)
                {
                    table.Cell().Text(p.ID_Producto.ToString());
                    table.Cell().Text(p.Referencia?.DescripReferencia);
                    table.Cell().Text(p.Tela?.DescripTela);
                    table.Cell().Text(p.Talla?.DescripTalla);
                    table.Cell().Text(p.ColorNav?.Nombre);
                    table.Cell().Text(p.Stock.ToString());
                }
            });
        }
    }
}
