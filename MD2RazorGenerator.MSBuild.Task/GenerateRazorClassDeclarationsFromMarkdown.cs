using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Toolbelt.Blazor.MD2Razor.Internals;

namespace Toolbelt.Blazor.MD2Razor;

public class GenerateRazorClassDeclarationsFromMarkdown : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[]? MarkdownFiles { get; set; }

    [Required]
    public string? OutputDir { get; set; }

    public string? RootNamespace { get; set; }

    public string? ProjectDir { get; set; }

    public string? DefaultBaseClass { get; set; }

    [Output]
    public ITaskItem[] Generated { get; set; } = [];

    public override bool Execute()
    {
        if (!Directory.Exists(this.OutputDir)) Directory.CreateDirectory(this.OutputDir);

        var globalOptions = new GlobalOptions(
            rootNamespace: this.RootNamespace ?? "",
            projectDir: this.ProjectDir ?? "",
            defaultBaseClass: this.DefaultBaseClass ?? ""
        );

        var md2razor = new MD2Razor();
        var generatedFilesPath = new List<string>();
        Parallel.ForEach(this.MarkdownFiles ?? [], markdownFile =>
        {
            // Determine the output path for the generated file
            var className = Path.GetFileNameWithoutExtension(markdownFile.ItemSpec).Replace('.', '_');
            var outputPath = Path.Combine(this.OutputDir ?? "", $"{className}.g.cs");
            generatedFilesPath.Add(outputPath);

            // Skip if the generated file is up to date
            var markdownTimestamp = File.GetLastWriteTime(markdownFile.ItemSpec);
            var outputTimestamp = File.Exists(outputPath) ? File.GetLastWriteTime(outputPath) : DateTime.MinValue;
            if (markdownTimestamp <= outputTimestamp) return;

            // Generate the Razor component class declaration code from the Markdown file
            var markdownText = File.ReadAllText(markdownFile.ItemSpec);
            var (_, generatedCode) = md2razor.GenerateCode(markdownFile.ItemSpec, markdownText, globalOptions, declarationOnly: true);
            File.WriteAllText(outputPath, generatedCode);
        });

        // Delete old generated files that are not in the current generation
        var existingFilesPath = Directory.GetFiles(this.OutputDir ?? "", "*.g.cs", SearchOption.TopDirectoryOnly);
        foreach (var file in existingFilesPath.Where(p1 => !generatedFilesPath.Any(p2 => p1.Equals(p2, StringComparison.InvariantCultureIgnoreCase))))
        {
            File.Delete(file);
        }

        this.Generated = generatedFilesPath.Select(p => new TaskItem(p)).ToArray();
        return true;
    }
}
