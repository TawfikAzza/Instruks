using Application.Interfaces.Content;
using Ganss.Xss;


namespace Infrastructure.Content
{
    /// <summary>
    /// Ganss.HtmlSanitizer-based implementation with a restricted tag/attribute set.
    /// </summary>
    public sealed class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitizerService()
        {
            _sanitizer = new HtmlSanitizer();

            _sanitizer.AllowedTags.Clear();
            _sanitizer.AllowedTags.UnionWith(new[]
            {
                "p","b","i","u","strong","em","ul","ol","li","br",
                "h1","h2","h3","blockquote","code","pre","a"
            });

            _sanitizer.AllowedAttributes.Clear();
            _sanitizer.AllowedAttributes.UnionWith(new[] { "href" });

            _sanitizer.AllowedSchemes.UnionWith(new[] { "https", "http" });
        }

        public string Sanitize(string? html) => _sanitizer.Sanitize(html ?? string.Empty);
    }
}