namespace Toolbelt.Blazor.MD2Razor.Internals;

internal static class PathUtils
{
    /// <summary>
    /// Normalizes a file path by replacing backslashes or forward slashes with current platform's directory separator.
    /// </summary>
    /// <param name="path">The file path to normalize.</param>
    public static string NormalizePath(string path) => NormalizePath(path, Path.DirectorySeparatorChar);

    /// <summary>
    /// Normalizes a file path by replacing backslashes or forward slashes with the specified directory separator.
    /// </summary>
    /// <param name="path">The file path to normalize.</param>
    /// <param name="pathSeparator">The character to use as the path separator.</param>
    /// <returns>The normalized file path.</returns>
    public static string NormalizePath(string path, char pathSeparator)
    {
        return string.Join(pathSeparator.ToString(), path.Split('\\', '/', ':'))
            .TrimEnd(pathSeparator);
    }
}