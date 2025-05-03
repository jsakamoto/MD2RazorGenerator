using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Toolbelt.Blazor.MD2Razor.Internals;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace Toolbelt.Blazor.MD2Razor;

public class MD2Razor
{
    /// <summary>
    /// The Markdown pipeline used for parsing Markdown content.
    /// </summary>
    private readonly MarkdownPipeline _markdownPipeline;

    public MD2Razor()
    {
        // Initialize the Markdown pipeline with YAML front matter support and other extensions.
        this._markdownPipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseEmojiAndSmiley()
            .UseAdvancedExtensions()
            .Build();
    }

    /// <summary>
    /// Generates C# source code from the provided Markdown content.
    /// </summary>
    /// <param name="markdownPath">The file path of the Markdown file.</param>
    /// <param name="markdownText">The content of the Markdown file.</param>
    /// <param name="imports">A collection of additional using directives to include in the generated code.</param>
    /// <param name="globalOptions">Global options such as the root namespace and project directory.</param>
    /// <param name="declarationOnly">If set to <c>true</c>, only the class declaration and its metadata (e.g., namespace and using directives) will be generated without the implementation of the <c>BuildRenderTree</c> method.</param>
    /// <returns>The generated source code.</returns>
    internal string GenerateCode(string markdownPath, string markdownText, IEnumerable<Imports> imports, GlobalOptions globalOptions, bool declarationOnly = false)
    {
        // Parse the Markdown content into a Markdown document using the configured pipeline.
        var markdownDoc = Markdown.Parse(markdownText, this._markdownPipeline);
        var frontMatter = ParseFrontMatter(markdownDoc);

        // Begin generating the source code for the Razor component.
        var sourceBuilder = new StringBuilder();

        // Generate the using directives based on the front matter.
        var usings = imports
            .GetApplicableImports(markdownPath)
            .SelectMany(i => i.GetUsings())
            .Concat(frontMatter.Usings)
            .OrderBy(ns => ns)
            .Distinct();
        foreach (var usingItem in usings)
        {
            sourceBuilder.AppendLine($"using {usingItem};");
        }
        if (frontMatter.Usings.Any()) sourceBuilder.AppendLine();

        // Generate the namespace for the generated class.
        var fullNamespace = frontMatter.Namespace ?? GenerateNamespaceFromPath(markdownPath, globalOptions);
        if (!string.IsNullOrEmpty(fullNamespace))
        {
            sourceBuilder.AppendLine($"namespace {fullNamespace};");
            sourceBuilder.AppendLine();
        }

        if (!declarationOnly)
        {
            // Generate the route attributes for the generated class.
            foreach (var page in frontMatter.Pages)
            {
                sourceBuilder.AppendLine($"[global::Microsoft.AspNetCore.Components.RouteAttribute(\"{page}\")]");
            }

            // Generate the layout attributes for the generated class.
            if (!string.IsNullOrWhiteSpace(frontMatter.Layout))
            {
                sourceBuilder.AppendLine($"[global::Microsoft.AspNetCore.Components.LayoutAttribute(typeof({frontMatter.Layout}))]");
            }

            // Generate other attributes for the generated class.
            foreach (var attrib in frontMatter.Attributes)
            {
                sourceBuilder.AppendLine($"[{attrib}]");
            }
        }

        // Generate the class name and base class for the generated Razor component.
        var className = Path.GetFileNameWithoutExtension(markdownPath).Replace('.', '_');
        var baseClass = frontMatter.Inherit ?? globalOptions.DefaultBaseClass;

        // Generate the class definition for the Razor component.
        var n = 0;
        sourceBuilder.AppendLine($"public partial class {className} : {baseClass}");
        sourceBuilder.AppendLine("{");

        if (!declarationOnly)
        {
            sourceBuilder.AppendLine("    protected override void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)");
            sourceBuilder.AppendLine("    {");

            if (!string.IsNullOrWhiteSpace(frontMatter.PageTitle))
            {
                sourceBuilder.AppendLine($"        __builder.OpenComponent<global::Microsoft.AspNetCore.Components.Web.PageTitle>({n++});");
                sourceBuilder.AppendLine($"        __builder.AddAttribute({n++}, \"ChildContent\", (global::Microsoft.AspNetCore.Components.RenderFragment)((__builder2) => {{");
                sourceBuilder.AppendLine($"            __builder2.AddContent({n++}, @\"{frontMatter.PageTitle?.Replace("\"", "\"\"")}\");");
                sourceBuilder.AppendLine($"        }}));");
                sourceBuilder.AppendLine($"        __builder.CloseComponent();");
                sourceBuilder.AppendLine();
            }

            sourceBuilder.Append($"        __builder.AddMarkupContent({n++}, @\"");

            // Update the Markdown document to add target="_blank" to external links.
            foreach (var node in markdownDoc.Descendants())
            {
                var url =
                    node is AutolinkInline autoLink ? autoLink.Url :
                    node is LinkInline link ? link.Url :
                    default;
                if (string.IsNullOrEmpty(url)) continue;

                if (Regex.IsMatch(url, "^[a-z]+://"))
                {
                    var attributes = node.GetAttributes();
                    attributes.AddPropertyIfNotExist("target", "_blank");
                }
            }

            // Convert the Markdown document to HTML using the configured pipeline.
            var html = markdownDoc.ToHtml(this._markdownPipeline).Replace("\"", "\"\"");
            sourceBuilder.Append(html);

            sourceBuilder.AppendLine("\");");
            sourceBuilder.AppendLine("    }"); // End of BuildRenderTree method
        }

        sourceBuilder.AppendLine("}"); // End of class

        return sourceBuilder.ToString();
    }

    /// <summary>
    /// Parses the YAML front matter from the provided Markdown document.
    /// </summary>
    /// <param name="markdownDoc">The Markdown document to parse.</param>
    /// <returns>A <see cref="FrontMatter"/> object containing the parsed front matter.</returns>
    private static FrontMatter ParseFrontMatter(MarkdownDocument markdownDoc)
    {
        var yamlBlock = markdownDoc.OfType<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock is null) return new FrontMatter();

        markdownDoc.Remove(yamlBlock);

        var yamlText = yamlBlock.Lines.ToString().Trim('-', '\r', '\n');
        using var reader = new StringReader(yamlText);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        var yamlNodeRoot = yamlStream.Documents.FirstOrDefault()?.RootNode as YamlMappingNode;
        var yamlNodeMap = yamlNodeRoot?.Children;

        static IEnumerable<string> getFrontMatterEntry(IOrderedDictionary<YamlNode, YamlNode>? nodeMap, string key)
        {
            if (nodeMap?.TryGetValue(new YamlScalarNode(key), out var node) != true) return [];
            if (node is YamlSequenceNode sequence)
            {
                return sequence.Select(s => s.ToString()).ToArray();
            }
            return [node.ToString()];
        }

        return new FrontMatter(
            pages: getFrontMatterEntry(yamlNodeMap, "url"),
            usings: getFrontMatterEntry(yamlNodeMap, "$using"),
            namespaces: getFrontMatterEntry(yamlNodeMap, "$namespace"),
            attributes: getFrontMatterEntry(yamlNodeMap, "$attribute"),
            layouts: getFrontMatterEntry(yamlNodeMap, "$layout"),
            inherits: getFrontMatterEntry(yamlNodeMap, "$inherit"),
            pageTitles: getFrontMatterEntry(yamlNodeMap, "title")
        );
    }

    /// <summary>
    /// Generates a namespace for the generated class based on the provided parameters.
    /// </summary>
    /// <param name="markdownPath">The file path of the Markdown file.</param>
    /// <param name="globalOptions">Global options such as the root namespace and project directory.</param>
    /// <returns>The generated namespace as a string.</returns>
    private static string GenerateNamespaceFromPath(string markdownPath, GlobalOptions globalOptions)
    {
        // Get the relative path of the file from the project directory.
        var relativePath =
            !string.IsNullOrWhiteSpace(globalOptions.ProjectDir) && markdownPath.StartsWith(globalOptions.ProjectDir) ?
            markdownPath.Substring(globalOptions.ProjectDir.Length) :
            Path.GetFileName(markdownPath);

        // Convert the relative path to a namespace format.
        var namespacePath = Path.GetDirectoryName(relativePath)?.Replace("/", ".").Replace("\\", ".") ?? "";

        // Combine the root namespace and the relative path.
        return (globalOptions.RootNamespace + "." + namespacePath).Trim('.');
    }

    /// <summary>
    /// Transforms a file path to a dot-separated path based on the provided base path.
    /// </summary>
    /// <remarks>When the file path is "/foo/bar/baz.txt" and the base path is "/foo", the result will be "bar.baz.txt".</remarks>
    /// <param name="path">A file path to transform. This can be a relative or absolute path.</param>
    /// <param name="basePath">A base path to use for the transformation. This should be a directory path.</param>
    /// <returns>returns a dot-separated path that represents the file's location relative to the base path.</returns>
    internal static string TransformToDotSeparatedPath(string path, string basePath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(basePath)) return path;

        basePath = PathUtils.NormalizePath(basePath, '.');
        path = PathUtils.NormalizePath(path, '.');

        if (path.StartsWith(basePath, StringComparison.InvariantCultureIgnoreCase))
        {
            return path.Substring(basePath.Length).TrimStart('.');
        }
        return path;
    }
}
