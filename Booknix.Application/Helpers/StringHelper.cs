using System.Text;
using System.Text.RegularExpressions;

namespace Booknix.Application.Helpers;

public static class StringHelper
{
    public static string ToUrlFriendly(string text)
    {
        var map = new Dictionary<char, string>
        {
            ['ç'] = "c", ['Ç'] = "c",
            ['ğ'] = "g", ['Ğ'] = "g",
            ['ı'] = "i", ['İ'] = "i",
            ['ö'] = "o", ['Ö'] = "o",
            ['ş'] = "s", ['Ş'] = "s",
            ['ü'] = "u", ['Ü'] = "u"
        };

        var normalized = text.Trim().ToLowerInvariant();

        var builder = new StringBuilder();
        foreach (var c in normalized)
        {
            if (map.ContainsKey(c))
                builder.Append(map[c]);
            else if (char.IsLetterOrDigit(c))
                builder.Append(c);
            else if (char.IsWhiteSpace(c) || c == '-')
                builder.Append('-');
            // özel karakterleri yok say
        }

        // birden fazla tireyi teke indir
        return Regex.Replace(builder.ToString(), "-+", "-").Trim('-');
    }
}
