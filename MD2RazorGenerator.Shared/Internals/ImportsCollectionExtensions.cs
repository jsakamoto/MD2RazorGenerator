namespace Toolbelt.Blazor.MD2Razor.Internals;

/// <summary>
/// Provides extension methods for collections of <see cref="Imports"/> objects.
/// </summary>
internal static class ImportsCollectionExtensions
{
    /// <summary>
    /// Filters a collection of imports to find those that apply to a given file path based on directory hierarchy.
    /// </summary>
    /// <param name="importsCollection">The collection of <see cref="Imports"/> to filter.</param>
    /// <param name="path">The file path to find applicable imports for.</param>
    /// <returns>A filtered collection of <see cref="Imports"/> that are applicable to the specified path.</returns>
    /// <remarks>
    /// An import is considered "applicable" if the directory containing the specified path
    /// starts with the directory containing the import's path. This matches Razor's cascading
    /// import behavior where _Imports.razor files apply to the directory they're in and all subdirectories.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the provided path is invalid.</exception>
    public static IEnumerable<Imports> GetApplicableImports(this IEnumerable<Imports> importsCollection, string path)
    {
        var baseDir = Path.GetDirectoryName(PathUtils.NormalizePath(path)) ?? throw new ArgumentException($"Invalid path: {path}");
        return importsCollection
            .Where(i => baseDir.StartsWith(Path.GetDirectoryName(PathUtils.NormalizePath(i.Path)), StringComparison.InvariantCultureIgnoreCase));
    }
}