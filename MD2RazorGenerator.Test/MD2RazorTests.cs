using Toolbelt.Blazor.MD2Razor;
using Toolbelt.Blazor.MD2Razor.Internals;

namespace MD2RazorGenerator.Test;

public class MD2RazorTests
{
    private static string NormalizePath(string path) => string.Join(Path.DirectorySeparatorChar, path.Split('/'));

    private static readonly GlobalOptions _globalOptions = new(
        rootNamespace: "Toolbelt.Samples",
        projectDir: NormalizePath("/home/user/projects/sample001/"),
        defaultBaseClass: "global::Microsoft.AspNetCore.Components.ComponentBase"
    );

    [Test]
    public void GenerateNamespaceFromPath_RootPath_Test()
    {
        // Given
        var markdownPath = NormalizePath("/home/user/projects/sample001/001.md");

        // When
        var generatedNamespace = MD2Razor.GenerateNamespaceFromPath(markdownPath, _globalOptions);

        // Then
        generatedNamespace.Is("Toolbelt.Samples");
    }

    [Test]
    public void GenerateNamespaceFromPath_SubPath_Test()
    {
        // Given
        var markdownPath = NormalizePath("/home/user/projects/sample001/Pages/001.md");

        // When
        var generatedNamespace = MD2Razor.GenerateNamespaceFromPath(markdownPath, _globalOptions);

        // Then
        generatedNamespace.Is("Toolbelt.Samples.Pages");
    }

    [Test]
    public void GenerateNamespaceFromPath_with_InvalidChars_Test()
    {
        // Given
        var markdownPath = NormalizePath("/home/user/projects/sample001/Posts/2025+01/Jan@Sapporo/001.md");

        // When
        var generatedNamespace = MD2Razor.GenerateNamespaceFromPath(markdownPath, _globalOptions);

        // Then
        generatedNamespace.Is("Toolbelt.Samples.Posts._2025_01.Jan_Sapporo");
    }
}
