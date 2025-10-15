namespace Application.Interfaces.Content
{
    /// <summary>
    /// Sanitizes rich HTML content using a whitelist approach.
    /// </summary>
    public interface IHtmlSanitizerService
    {
        /// <summary>Return a sanitized version of the given HTML (never returns null).</summary>
        string Sanitize(string? html);
    }
}