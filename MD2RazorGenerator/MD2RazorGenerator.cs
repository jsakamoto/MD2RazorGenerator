using Microsoft.CodeAnalysis;
using Toolbelt.Blazor.MD2Razor.Internals;

namespace Toolbelt.Blazor.MD2Razor;

/// <summary>
/// A source generator that converts Markdown (.md) files into Razor components.
/// </summary>
[Generator(LanguageNames.CSharp)]
public partial class MD2RazorGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator with the provided context.
    /// </summary>
    /// <param name="context">The initialization context for the incremental generator.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter additional text files to include only Markdown (.md) files.
        var markdownFiles = context.AdditionalTextsProvider.Where(t => t.Path.EndsWith(".md"));

        // Create a provider to retrieve global options such as the root namespace and project directory.
        var globalOptionsProvider = GetGlobalOptionsProvider(context);

        // Register the source generation logic.
        var md2razor = new MD2Razor();
        context.RegisterSourceOutput(markdownFiles.Combine(globalOptionsProvider), (context, pair) =>
        {
            var (markdownFile, globalOptions) = pair;
            var markdownText = markdownFile.GetText(context.CancellationToken)?.ToString();
            if (markdownText is null) return;

            var generatedCode = md2razor.GenerateCode(markdownFile.Path, markdownText, globalOptions);
            var hintName = MD2Razor.TransformToDotSeparatedPath(markdownFile.Path, globalOptions.ProjectDir) + ".g.cs";
            context.AddSource(hintName, generatedCode);
        });
    }

    /// <summary>
    /// Creates an incremental value provider for retrieving global options.
    /// </summary>
    /// <param name="context">The initialization context for the incremental generator.</param>
    /// <returns>An incremental value provider for global options.</returns>
    private static IncrementalValueProvider<GlobalOptions> GetGlobalOptionsProvider(IncrementalGeneratorInitializationContext context)
    {
        return context
            .AnalyzerConfigOptionsProvider
            .Select((options, _) => new GlobalOptions(
                rootNamespace: options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace) ? rootNamespace : "",
                projectDir: options.GlobalOptions.TryGetValue("build_property.ProjectDir", out var projectDir) ? projectDir : "",
                defaultBaseClass: options.GlobalOptions.TryGetValue("build_property.MD2RazorDefaultBaseClass", out var defaultBaseClass) ? defaultBaseClass : ""
            ));
    }
}
