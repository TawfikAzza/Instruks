using Application.Interfaces;
using Application.Interfaces;
using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Ganss.Xss;

namespace Application.Services
{
    public class InstruksPdfService : IInstruksPdfService
    {
        private readonly IInstruksRepository _repository;

        public InstruksPdfService(IInstruksRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]> GeneratePdfAsync(Guid instruksId)
        {
            var instruks = await _repository.GetWithCategoryAsync(instruksId);
            if (instruks == null)
                throw new Exception("Instruks not found");

            // Sanitize Quill HTML (removes dangerous tags)
            var sanitizer = new HtmlSanitizer();
            var safeHtml = sanitizer.Sanitize(instruks.Content ?? string.Empty);

            // Build PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().Text(instruks.Title).FontSize(18).Bold();
                            if (instruks.Category != null)
                                col.Item().Text($"Category: {instruks.Category.Name}");
                            if (!string.IsNullOrWhiteSpace(instruks.Description))
                                col.Item().Text(instruks.Description).Italic();
                        });

                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Element(c =>
                            {
                                RenderHtml(c, safeHtml);
                            });
                        });

                    page.Footer()
                        .AlignRight()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

       private static void RenderHtml(IContainer container, string html)
{
    var doc = new HtmlDocument();
    doc.LoadHtml(html);
    var body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;

    container.Column(col =>
    {
        foreach (var node in body.ChildNodes)
            RenderNode(col, node);
    });
}

private static void RenderNode(ColumnDescriptor col, HtmlNode node)
{
    var name = node.Name.ToLowerInvariant();

    switch (name)
    {
        case "#text":
            {
                var t = node.InnerText.Trim();
                if (!string.IsNullOrEmpty(t))
                    col.Item().Text(t);
                break;
            }

        case "br":
            col.Item().Height(6);
            break;

        case "p":
            col.Item().PaddingBottom(6).Text(node.InnerText.Trim()).LineHeight(1.3f);
            break;

        case "h1":
            col.Item().PaddingBottom(6).Text(node.InnerText.Trim()).FontSize(18).Bold();
            break;

        case "h2":
            col.Item().PaddingBottom(6).Text(node.InnerText.Trim()).FontSize(16).SemiBold();
            break;

        case "h3":
            col.Item().PaddingBottom(6).Text(node.InnerText.Trim()).FontSize(14).SemiBold();
            break;

        case "blockquote":
            col.Item()
               .PaddingLeft(10).PaddingVertical(4)
               .BorderLeft(2).BorderColor(Colors.Grey.Lighten2)
               .Text(node.InnerText.Trim()).Italic().FontColor(Colors.Grey.Darken2);
            break;

        case "ul":
            AddUnorderedList(col, node);
            break;

        case "ol":
            AddOrderedList(col, node);
            break;

        default:
            if (node.HasChildNodes)
                foreach (var child in node.ChildNodes)
                    RenderNode(col, child);
            break;
    }
}

private static void AddUnorderedList(ColumnDescriptor col, HtmlNode ul)
{
    var items = ul.SelectNodes("./li");
    if (items == null) return;

    col.Item().PaddingBottom(6).Column(list =>
    {
        foreach (var li in items)
        {
            list.Item().Row(r =>
            {
                r.AutoItem().Text("• ");
                r.RelativeItem().Text(li.InnerText.Trim());
            });
        }
    });
}

private static void AddOrderedList(ColumnDescriptor col, HtmlNode ol)
{
    var items = ol.SelectNodes("./li");
    if (items == null) return;

    int index = 1;
    col.Item().PaddingBottom(6).Column(list =>
    {
        foreach (var li in items)
        {
            var n = index++;
            list.Item().Row(r =>
            {
                r.AutoItem().Text($"{n}. ");
                r.RelativeItem().Text(li.InnerText.Trim());
            });
        }
    });
}
    }
}
