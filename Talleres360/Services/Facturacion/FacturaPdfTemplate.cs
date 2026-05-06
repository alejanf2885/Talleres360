using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Talleres360.Models.Facturacion;

namespace Talleres360.Services.Facturacion
{
    public class FacturaPdfTemplate : IDocument
    {
        private readonly Factura _factura;
        private readonly List<LineaFactura> _lineas;
        private readonly List<DesgloseIva> _desgloses;
        private readonly byte[]? _logoBytes;

        private static readonly string ColorPrimario = "#1E3A5F";
        private static readonly string ColorSecundario = "#F0F4F8";
        private static readonly string ColorBorde = "#CBD5E0";

        public FacturaPdfTemplate(Factura factura, List<LineaFactura> lineas, List<DesgloseIva> desgloses, byte[]? logoBytes = null)
        {
            _factura   = factura;
            _lineas    = lineas;
            _desgloses = desgloses;
            _logoBytes = logoBytes;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Factura {_factura.NumeroFactura}",
            Author = _factura.TallerNombre ?? "Talleres360",
            CreationDate = _factura.FechaEmision
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily("Arial"));

                page.Header().Element(ComponerCabecera);
                page.Content().Element(ComponerContenido);
                page.Footer().Element(ComponerPie);
            });
        }

        private void ComponerCabecera(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Datos del taller
                    row.RelativeItem(2).Column(c =>
                    {
                        if (_logoBytes != null && _logoBytes.Length > 0)
                        {
                            c.Item().PaddingBottom(4).Width(60).Image(_logoBytes).FitWidth();
                        }
                        c.Item().Text(_factura.TallerNombre ?? string.Empty)
                            .FontSize(14).Bold().FontColor(ColorPrimario);
                        if (!string.IsNullOrWhiteSpace(_factura.TallerCif))
                            c.Item().Text($"CIF: {_factura.TallerCif}").FontSize(8).FontColor("#555555");
                        if (!string.IsNullOrWhiteSpace(_factura.TallerDireccion))
                            c.Item().Text(_factura.TallerDireccion).FontSize(8).FontColor("#555555");
                        if (!string.IsNullOrWhiteSpace(_factura.TallerLocalidad))
                            c.Item().Text(_factura.TallerLocalidad).FontSize(8).FontColor("#555555");
                        if (!string.IsNullOrWhiteSpace(_factura.TallerTelefono))
                            c.Item().Text($"Tel: {_factura.TallerTelefono}").FontSize(8).FontColor("#555555");
                    });

                    // Datos del documento
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Background(ColorPrimario).Padding(10).Column(inner =>
                        {
                            inner.Item().Text(TipoDocumentoTexto())
                                .FontSize(13).Bold().FontColor(Colors.White).AlignRight();
                            inner.Item().Text(_factura.NumeroFactura)
                                .FontSize(11).FontColor(Colors.White).AlignRight();
                            inner.Item().Text($"Fecha: {_factura.FechaEmision:dd/MM/yyyy}")
                                .FontSize(8).FontColor(Colors.White).AlignRight();
                            if (_factura.FechaVencimiento.HasValue)
                                inner.Item().Text($"Vence: {_factura.FechaVencimiento.Value:dd/MM/yyyy}")
                                    .FontSize(8).FontColor(Colors.White).AlignRight();
                        });
                    });
                });

                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(ColorPrimario);
            });
        }

        private void ComponerContenido(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(12);

                // Datos cliente + estado factura
                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Border(1).BorderColor(ColorBorde).Padding(8).Column(c =>
                    {
                        c.Item().Text("DATOS DEL CLIENTE").FontSize(8).Bold().FontColor(ColorPrimario);
                        c.Item().PaddingTop(4).Text(_factura.ClienteNombre).Bold();
                        if (!string.IsNullOrWhiteSpace(_factura.ClienteNifCif) && _factura.ClienteNifCif != "-")
                            c.Item().Text($"NIF/CIF: {_factura.ClienteNifCif}").FontSize(8);
                        if (!string.IsNullOrWhiteSpace(_factura.ClienteDireccion))
                            c.Item().Text(_factura.ClienteDireccion).FontSize(8);
                        if (!string.IsNullOrWhiteSpace(_factura.ClienteCodigoPostal) || !string.IsNullOrWhiteSpace(_factura.ClienteLocalidad))
                            c.Item().Text($"{_factura.ClienteCodigoPostal} {_factura.ClienteLocalidad}".Trim()).FontSize(8);
                        if (!string.IsNullOrWhiteSpace(_factura.ClienteEmail))
                            c.Item().Text(_factura.ClienteEmail).FontSize(8);
                        if (!string.IsNullOrWhiteSpace(_factura.ClienteTelefono))
                            c.Item().Text($"Tel: {_factura.ClienteTelefono}").FontSize(8);
                    });

                    row.ConstantItem(12);

                    row.RelativeItem().Border(1).BorderColor(ColorBorde).Padding(8).Column(c =>
                    {
                        c.Item().Text("DATOS DE LA FACTURA").FontSize(8).Bold().FontColor(ColorPrimario);
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("Estado pago:").FontSize(8).FontColor("#555555");
                            r.RelativeItem().Text(_factura.EstadoPago).FontSize(8).Bold().AlignRight();
                        });
                        if (!string.IsNullOrWhiteSpace(_factura.MetodoPago))
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Método pago:").FontSize(8).FontColor("#555555");
                                r.RelativeItem().Text(_factura.MetodoPago).FontSize(8).AlignRight();
                            });
                        if (!string.IsNullOrWhiteSpace(_factura.Serie))
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Serie:").FontSize(8).FontColor("#555555");
                                r.RelativeItem().Text(_factura.Serie).FontSize(8).AlignRight();
                            });
                    });
                });

                // Tabla de líneas
                col.Item().Element(ComponerTablaLineas);

                // Totales + desglose IVA
                col.Item().Element(ComponerTotales);

                // Notas legales
                if (!string.IsNullOrWhiteSpace(_factura.NotasLegales))
                {
                    col.Item().Border(1).BorderColor(ColorBorde).Padding(8).Column(c =>
                    {
                        c.Item().Text("NOTAS").FontSize(8).Bold().FontColor(ColorPrimario);
                        c.Item().PaddingTop(4).Text(_factura.NotasLegales).FontSize(8).FontColor("#555555");
                    });
                }
            });
        }

        private void ComponerTablaLineas(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(4);  // Concepto
                    cols.RelativeColumn(1);  // Cantidad
                    cols.RelativeColumn(1.5f); // Precio
                    cols.RelativeColumn(1);  // Dto%
                    cols.RelativeColumn(1);  // IVA%
                    cols.RelativeColumn(1.5f); // Total
                });

                // Cabecera
                table.Header(header =>
                {
                    string[] titulos = { "Concepto", "Cant.", "P. Unit.", "Dto.%", "IVA%", "Total" };
                    bool[] derechas = { false, true, true, true, true, true };

                    for (int i = 0; i < titulos.Length; i++)
                    {
                        int idx = i;
                        TextBlockDescriptor txt = header.Cell().Background(ColorPrimario).Padding(5)
                            .Text(titulos[idx]).FontSize(8).Bold().FontColor(Colors.White);
                        if (derechas[idx]) txt.AlignRight();
                    }
                });

                // Filas
                foreach (LineaFactura linea in _lineas)
                {
                    bool esImpar = _lineas.IndexOf(linea) % 2 == 0;
                    string fondo = esImpar ? ColorSecundario : Colors.White;

                    table.Cell().Background(fondo).Padding(5).Text(linea.Concepto).FontSize(8);
                    table.Cell().Background(fondo).Padding(5).AlignRight().Text(linea.Cantidad.ToString("N2")).FontSize(8);
                    table.Cell().Background(fondo).Padding(5).AlignRight().Text($"{linea.PrecioUnitario:N2} €").FontSize(8);
                    table.Cell().Background(fondo).Padding(5).AlignRight().Text(linea.DescuentoPorcentaje > 0 ? $"{linea.DescuentoPorcentaje:N0}%" : "-").FontSize(8);
                    table.Cell().Background(fondo).Padding(5).AlignRight().Text($"{linea.ImpuestoPorcentaje:N0}%").FontSize(8);
                    table.Cell().Background(fondo).Padding(5).AlignRight().Text($"{linea.TotalLinea:N2} €").FontSize(8).Bold();
                }
            });
        }

        private void ComponerTotales(IContainer container)
        {
            container.Row(row =>
            {
                // Desglose IVA
                row.RelativeItem().Column(col =>
                {
                    if (_desgloses.Count > 0)
                    {
                        col.Item().Text("DESGLOSE IVA").FontSize(8).Bold().FontColor(ColorPrimario);
                        col.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(ColorSecundario).Padding(4).Text("Base imp.").FontSize(7).Bold();
                                h.Cell().Background(ColorSecundario).Padding(4).AlignCenter().Text("IVA %").FontSize(7).Bold();
                                h.Cell().Background(ColorSecundario).Padding(4).AlignRight().Text("Cuota").FontSize(7).Bold();
                            });

                            foreach (DesgloseIva d in _desgloses)
                            {
                                table.Cell().BorderBottom(1).BorderColor(ColorBorde).Padding(4)
                                    .Text($"{d.BaseImponible:N2} €").FontSize(7);
                                table.Cell().BorderBottom(1).BorderColor(ColorBorde).Padding(4).AlignCenter()
                                    .Text($"{d.TipoIvaPorcentaje:N0}%").FontSize(7);
                                table.Cell().BorderBottom(1).BorderColor(ColorBorde).Padding(4).AlignRight()
                                    .Text($"{d.CuotaIva:N2} €").FontSize(7);
                            }
                        });
                    }
                });

                row.ConstantItem(20);

                // Resumen total
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal:").FontSize(9).FontColor("#555555");
                        r.ConstantItem(90).AlignRight().Text($"{_factura.Subtotal:N2} €").FontSize(9);
                    });
                    col.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text("IVA:").FontSize(9).FontColor("#555555");
                        r.ConstantItem(90).AlignRight().Text($"{_factura.ImporteImpuestos:N2} €").FontSize(9);
                    });
                    col.Item().PaddingTop(4).Background(ColorPrimario).Padding(6).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").FontSize(11).Bold().FontColor(Colors.White);
                        r.ConstantItem(90).AlignRight().Text($"{_factura.Total:N2} €").FontSize(11).Bold().FontColor(Colors.White);
                    });
                });
            });
        }

        private void ComponerPie(IContainer container)
        {
            container.PaddingTop(8).Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(ColorBorde);
                col.Item().PaddingTop(4).AlignCenter()
                    .Text($"Documento generado el {DateTime.Now:dd/MM/yyyy} — {_factura.TallerNombre}")
                    .FontSize(7).FontColor("#999999");
            });
        }

        private string TipoDocumentoTexto() => _factura.TipoDocumento.ToString() switch
        {
            "FACTURA" => "FACTURA",
            "PRESUPUESTO" => "PRESUPUESTO",
            "ALBARAN" => "ALBARÁN",
            "FACTURA_RECTIFICATIVA" => "FACTURA RECTIFICATIVA",
            _ => _factura.TipoDocumento.ToString()
        };
    }
}
