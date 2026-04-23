using System.Text.RegularExpressions;

namespace Api.Endpoints.Chat;

internal static partial class ChatHtmlSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "b", "strong", "i", "em", "u", "p", "br", "ul", "ol", "li", "a", "span"
    };

    public static string? Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var noScripts = ScriptTagRegex().Replace(html, string.Empty);
        var sanitized = HtmlTagRegex().Replace(noScripts, static match =>
        {
            var tag = TagNameRegex().Match(match.Value).Value;
            return AllowedTags.Contains(tag) ? match.Value : string.Empty;
        });

        return sanitized.Trim();
    }

    [GeneratedRegex(@"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"<\/?([a-zA-Z0-9]+)(\s[^>]*)?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"[a-zA-Z0-9]+")]
    private static partial Regex TagNameRegex();
}
