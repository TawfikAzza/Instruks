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

            // Sanitize Quill HTML (allow underline + simple inline styles)
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("u");
            sanitizer.AllowedAttributes.Add("style");
            sanitizer.AllowedCssProperties.Add("text-decoration");
            sanitizer.AllowedCssProperties.Add("font-weight");
            sanitizer.AllowedCssProperties.Add("font-style");
            sanitizer.AllowedCssProperties.Add("color");

            var safeHtml = sanitizer.Sanitize(instruks.Content ?? string.Empty);

            // --- load logo from disk ---
            var logoPath = Path.Combine(AppContext.BaseDirectory, "Services", "images", "idun-logo.png");
            byte[]? logoBytes = null;
            if (File.Exists(logoPath))
                logoBytes = File.ReadAllBytes(logoPath);

            // --- build PDF ---
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    // HEADER
                    page.Header().Element(header =>
                    {
                        header
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Grey.Lighten4)
                            .PaddingVertical(6)
                            .PaddingHorizontal(10)
                            .Row(row =>
                            {
                                // Left side: title and metadata
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(instruks.Title).FontSize(18).Bold();
                                    if (instruks.Category != null)
                                        col.Item().Text($"Category: {instruks.Category.Name}");
                                    if (!string.IsNullOrWhiteSpace(instruks.Description))
                                        col.Item().Text(instruks.Description).Italic();
                                });

                                // Right side: logo
                                if (logoBytes != null)
                                {
                                    row.ConstantItem(80)
                                        .AlignRight()
                                        .AlignMiddle()
                                        .Image(logoBytes);
                                }
                            });
                    });

                    // CONTENT
                    page.Content().Column(col =>
                    {
                        col.Item().Element(c => RenderHtml(c, safeHtml));
                    });

                    // FOOTER
                    page.Footer().AlignRight().Text(x =>
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


        // ---------- HTML -> PDF rendering ----------

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
                    col.Item().Height(2);
                    break;

                case "p":
                    col.Item()
                        .PaddingBottom(2)
                        .Text(t => RenderInline(t, node))
                        ;
                    break;

                case "h1":
                    col.Item()
                        .PaddingBottom(6)
                        .Text(t => RenderInline(t, node, TextStyle.Default.Size(18).Bold()));
                    break;

                case "h2":
                    col.Item()
                        .PaddingBottom(6)
                        .Text(t => RenderInline(t, node, TextStyle.Default.Size(16).SemiBold()));
                    break;

                case "h3":
                    col.Item()
                        .PaddingBottom(6)
                        .Text(t => RenderInline(t, node, TextStyle.Default.Size(14).SemiBold()));
                    break;

                case "blockquote":
                    col.Item()
                        .PaddingLeft(10).PaddingVertical(4)
                        .BorderLeft(2).BorderColor(Colors.Grey.Lighten2)
                        .Text(t => RenderInline(t, node, TextStyle.Default.Italic().FontColor(Colors.Grey.Darken2)));
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
                        r.RelativeItem().Text(t => RenderInline(t, li));
                    });
                }
            });
        }

        private static void AddOrderedList(ColumnDescriptor col, HtmlNode ol)
        {
            var items = ol.SelectNodes("./li");
            if (items == null) return;

            var index = 1;
            col.Item().PaddingBottom(6).Column(list =>
            {
                foreach (var li in items)
                {
                    var n = index++;
                    list.Item().Row(r =>
                    {
                        r.AutoItem().Text($"{n}. ");
                        r.RelativeItem().Text(t => RenderInline(t, li));
                    });
                }
            });
        }

        // ---------- Inline rendering (QuestPDF 2022.12.15 compatible) ----------

        private static void RenderInline(TextDescriptor text, HtmlNode node, TextStyle? baseStyle = null)
        {
            baseStyle ??= TextStyle.Default;

            foreach (var child in node.ChildNodes)
            {
                var name = child.Name.ToLowerInvariant();

                if (name == "#text")
                {
                    var content = HtmlEntity.DeEntitize(child.InnerText);
                    if (!string.IsNullOrWhiteSpace(content))
                        text.Span(content).Style(baseStyle);
                    continue;
                }

                var style = baseStyle;

                switch (name)
                {
                    case "b":
                    case "strong":
                        style = style.Bold();
                        break;

                    case "i":
                    case "em":
                        style = style.Italic();
                        break;

                    case "u":
                        style = style.Underline();
                        break;

                    case "s":
                    case "strike":
                    case "del":
                        style = style.Strikethrough();
                        break;

                    case "span":
                        style = ApplyInlineCss(style, child.GetAttributeValue("style", null));
                        break;

                    case "a":
                    {
                        // No clickable hyperlinks on Text() in 2022.12.15 → render visually
                        var visualLinkStyle = style.Underline().FontColor(Colors.Blue.Medium);

                        if (child.HasChildNodes)
                            RenderInline(text, child, visualLinkStyle);
                        else
                        {
                            var content = HtmlEntity.DeEntitize(child.InnerText);
                            if (!string.IsNullOrWhiteSpace(content))
                                text.Span(content).Style(visualLinkStyle);
                        }
                        continue;
                    }
                }

                if (child.HasChildNodes)
                    RenderInline(text, child, style);
                else
                {
                    var content = HtmlEntity.DeEntitize(child.InnerText);
                    if (!string.IsNullOrWhiteSpace(content))
                        text.Span(content).Style(style);
                }
            }
        }

        private static TextStyle ApplyInlineCss(TextStyle style, string? css)
        {
            if (string.IsNullOrWhiteSpace(css)) return style;

            var rules = css.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var rule in rules)
            {
                var parts = rule.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim().ToLowerInvariant();
                var val = parts[1].Trim().ToLowerInvariant();

                switch (key)
                {
                    case "text-decoration":
                        if (val.Contains("underline")) style = style.Underline();
                        if (val.Contains("line-through")) style = style.Strikethrough();
                        break;

                    case "font-weight":
                        if (val is "bold" or "700" or "600" or "800" or "900")
                            style = style.Bold();
                        break;

                    case "font-style":
                        if (val.Contains("italic")) style = style.Italic();
                        break;

                    case "color":
                        // very simple named-color support
                        if (val.Contains("blue"))  style = style.FontColor(Colors.Blue.Medium);
                        if (val.Contains("red"))   style = style.FontColor(Colors.Red.Medium);
                        if (val.Contains("green")) style = style.FontColor(Colors.Green.Medium);
                        break;
                }
            }

            return style;
        }
    }
}
