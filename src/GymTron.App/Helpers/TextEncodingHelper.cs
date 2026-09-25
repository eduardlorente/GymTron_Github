using System.Net;
using System.Text;

namespace GymTron.App.Helpers;

public static class TextEncodingHelper
{
    public static string Sanitize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // 1. Decode HTML entities (e.g. &eacute;, &#233;)
        string decoded = WebUtility.HtmlDecode(text);

        // 2. Fix UTF-8 interpreted as Latin1 / Windows-1252 mojibake (e.g. Ã© -> é, Ã³ -> ó, etc.)
        if (decoded.Contains('Ã') || decoded.Contains('Â') || decoded.Contains('â'))
        {
            try
            {
                byte[] bytes = Encoding.Latin1.GetBytes(decoded);
                string utf8 = Encoding.UTF8.GetString(bytes);
                if (!utf8.Contains('\uFFFD'))
                {
                    decoded = utf8;
                }
            }
            catch
            {
                // Fallback to decoded if byte conversion fails
            }
        }

        return decoded;
    }
}
