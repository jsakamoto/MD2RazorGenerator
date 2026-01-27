using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Toolbelt.Blazor.MD2Razor.Internals;

/// <summary>
/// Represents a markdown file with its path and content.
/// </summary>
internal class MarkDownFile : IEquatable<MarkDownFile?>
{
    /// <summary>
    /// Gets the file path of an individual "*.md" file.
    /// </summary>
    public string Path { get; }

    private string? _text;

    /// <summary>
    /// The raw text content of the "*.md" file.
    /// </summary>
    public string Text { get => this._text ??= this._sourceText?.ToString() ?? ""; }

    /// <summary>
    /// The SourceText content object of the "*.md" file.
    /// </summary>
    private readonly SourceText? _sourceText;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkDownFile"/> class.
    /// </summary>
    /// <param name="path">The file path of the "*.md" file.</param>
    /// <param name="sourceText">The SourceText content object of the "*.md" file, or null if not available.</param>
    public MarkDownFile(string path, SourceText? sourceText)
    {
        this.Path = path;
        this._sourceText = sourceText;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as MarkDownFile);
    }

    /// <summary>
    /// Determines whether the specified MarkDownFile instance is equal to the current instance.
    /// </summary>
    /// <param name="other">The MarkDownFile instance to compare with the current instance.</param>
    /// <returns>true if the specified instance is equal to the current instance; otherwise, false.</returns>
    public bool Equals(MarkDownFile? other)
    {
        return other is not null &&
            this.Path == other.Path &&
            (
                (other._sourceText is not null && this._sourceText is not null && other._sourceText.ContentEquals(this._sourceText)) ||
                (other._sourceText is null && this._sourceText is null)
            );
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        var hashCode = 833777843;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Path);
        hashCode = hashCode * -1521134295 + EqualityComparer<ImmutableArray<byte>?>.Default.GetHashCode(this._sourceText?.GetChecksum());
        return hashCode;
    }
}