using GymTracker.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace GymTracker.Reports
{
    public static class ReporteService
    {
        public static void GenerarInforme(Cliente cliente, Ejercicio ejercicio, List<Seguimiento> registros)
        {
            var fileName = $"GymTracker_{Sanitizar(cliente.Nombre)}_{Sanitizar(ejercicio.Nombre)}_{DateTime.Now:yyyyMMdd}.pdf";
            var path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("GymTracker").FontSize(26).Bold().FontColor("#1565C0");
                                c.Item().Text("Informe de Entrenamiento").FontSize(13).FontColor("#607D8B");
                            });
                            row.ConstantItem(150).AlignRight().AlignMiddle()
                               .Text(DateTime.Now.ToString("dd/MM/yyyy")).FontColor("#9E9E9E");
                        });

                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Cliente: ").Bold();
                                t.Span(cliente.Nombre);
                            });
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Ejercicio: ").Bold();
                                t.Span(ejercicio.Nombre);
                            });
                        });

                        col.Item().PaddingTop(6).LineHorizontal(2).LineColor("#1565C0");
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        if (registros.Count == 0)
                        {
                            col.Item().PaddingTop(20).AlignCenter()
                               .Text("No hay registros para este ejercicio.")
                               .Italic().FontColor("#9E9E9E");
                            return;
                        }

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(45);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                foreach (var h in new[] { "#", "Fecha", "Peso (kg)", "Repeticiones" })
                                    header.Cell().Background("#1565C0").Padding(8)
                                          .Text(h).FontColor(Colors.White).Bold();
                            });

                            for (int i = 0; i < registros.Count; i++)
                            {
                                var r = registros[i];
                                var bg = i % 2 == 0 ? "#FFFFFF" : "#F5F5F5";
                                table.Cell().Background(bg).Padding(8).Text((i + 1).ToString());
                                table.Cell().Background(bg).Padding(8).Text(r.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Background(bg).Padding(8).Text($"{r.Peso:F1}");
                                table.Cell().Background(bg).Padding(8).Text(r.Repeticiones.ToString());
                            }
                        });

                        col.Item().PaddingTop(20).Column(resumen =>
                        {
                            resumen.Item().Text("Resumen").FontSize(13).Bold().FontColor("#1565C0");
                            resumen.Item().PaddingTop(6).Text($"Total de sesiones: {registros.Count}").Bold();
                            resumen.Item().Text($"Peso máximo: {registros.Max(r => r.Peso):F1} kg");
                            resumen.Item().Text($"Peso promedio: {registros.Average(r => r.Peso):F1} kg");
                            resumen.Item().Text($"Primera sesión: {registros.Min(r => r.Fecha):dd/MM/yyyy}");
                            resumen.Item().Text($"Última sesión: {registros.Max(r => r.Fecha):dd/MM/yyyy}");
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("GymTracker  —  Página ").FontColor("#9E9E9E");
                        x.CurrentPageNumber().FontColor("#9E9E9E");
                        x.Span(" de ").FontColor("#9E9E9E");
                        x.TotalPages().FontColor("#9E9E9E");
                    });
                });
            }).GeneratePdf(path);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }

        private static string Sanitizar(string nombre)
            => string.Concat(nombre.Split(System.IO.Path.GetInvalidFileNameChars())).Replace(' ', '_');
    }
}
