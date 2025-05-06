using System.Text.RegularExpressions;

namespace Toolbelt.Blazor.MD2Razor.Internals;

/// <summary>
/// Represents a collection of using statements from a specific file path.
/// Used to track and extract @using directives from "_Import.razor"" files.
/// </summary>
internal class Imports : IEquatable<Imports?>
{
    /// <summary>
    /// Gets the file path of an individual "_Imports.razor" file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The raw text content of the "_Imports.razor" file.
    /// </summary>
    private string _text;

    /// <summary>
    /// Cached collection of using namespace statements extracted from the text.
    /// </summary>
    private IEnumerable<string>? _usings = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Imports"/> class.
    /// </summary>
    /// <param name="path">The file path of the "_Imports.razor" file that contains the "@using" directives.</param>
    /// <param name="text">The text content of the "_Imports.razor" file, or null if not available.</param>
    public Imports(string path, string? text)
    {
        this.Path = path;
        this._text = text ?? "";
    }

    /// <summary>
    /// Extracts and returns the namespaces come from @using directives in the text content.
    /// </summary>
    /// <returns>A collection of namespace strings from @using directives.</returns>
    public IEnumerable<string> GetUsings()
    {
        return this._usings ??= this._text.Split(['\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => Regex.Match(line, @"^[ \t]*@using[ \t]+(?<namespace>(static[ \t]+)?[^ \t;]+).*$"))
            .Where(match => match.Success)
            .Select(match => match.Groups["namespace"].Value.Trim())
            .ToArray();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Imports);
    }

    /// <summary>
    /// Determines whether the specified Imports instance is equal to the current instance.
    /// </summary>
    /// <param name="other">The Imports instance to compare with the current instance.</param>
    /// <returns>true if the specified instance is equal to the current instance; otherwise, false.</returns>
    public bool Equals(Imports? other)
    {
        return other is not null &&
               this.Path == other.Path &&
               this._text == other._text;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hashCode = 833777843;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Path);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this._text);
        return hashCode;
    }
}