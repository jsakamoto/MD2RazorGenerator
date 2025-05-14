using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.MD2Razor.Internals;

internal static class ValidIdentifier
{
    private static readonly HashSet<string> _reservedWords = new([
        // https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/
        "add",        "field",   "nint",     "scoped",
        "allows",     "file",    "not",      "select",
        "alias",      "from",    "notnull",  "set",
        "and",        "get",     "nuint",    "unmanaged",
        "ascending",  "global",  "on",       "value",
        "args",       "group",   "or",       "var",
        "async",      "init",    "orderby",  "when",
        "await",      "into",    "partial",  "where",
        "by",         "join",    "record",   "with",
        "descending", "let",     "remove",   "yield",
        "dynamic",    "managed", "required",
        "equals",     "nameof",
        "extension",
    ]);

    public static string Create(string? name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (name.Length == 0) return name;

        if (Regex.IsMatch(name[0].ToString(), @"^\p{Nd}")) name = "_" + name;
        name = Regex.Replace(name, @"^[^\p{L}\p{Nl}_]", "_");
        name = Regex.Replace(name, @"[^\p{L}\p{Mn}\p{Mc}\p{Nd}\p{Nl}\p{Pc}_]", "_");

        if (_reservedWords.Contains(name)) name += "_";

        return name;
    }
}