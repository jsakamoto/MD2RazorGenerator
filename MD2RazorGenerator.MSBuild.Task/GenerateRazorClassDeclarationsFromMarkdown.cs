using System.Text;
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

        // Check if the global options have changed since the last generation
        // If they have, we need to regenerate all files.
        var globalOptionsHasBeenChanged = this.CheckGlobalOptionsHasBeenChanged(globalOptions);

        var md2razor = new MD2Razor();
        var generatedFilesPath = new List<string>();
        Parallel.ForEach(this.MarkdownFiles ?? [], markdownFile =>
        {
            var markdownFilePath = markdownFile.ItemSpec;

            // Determine the output path for the generated file
            var hintName = MD2Razor.TransformToDotSeparatedPath(markdownFilePath, this.ProjectDir ?? "") + ".g.cs";
            var outputPath = Path.Combine(this.OutputDir ?? "", hintName);
            generatedFilesPath.Add(outputPath);

            // Skip if the generated file is up to date as long as the global options have not changed
            if (!globalOptionsHasBeenChanged)
            {
                var markdownTimestamp = File.GetLastWriteTime(markdownFilePath);
                var outputTimestamp = File.Exists(outputPath) ? File.GetLastWriteTime(outputPath) : DateTime.MinValue;
                if (markdownTimestamp <= outputTimestamp) return;
            }

            // Generate the Razor component class declaration code from the Markdown file
            var markdownText = File.ReadAllText(markdownFilePath);
            var generatedCode = md2razor.GenerateCode(markdownFilePath, markdownText, globalOptions, declarationOnly: true);
            File.WriteAllText(outputPath, generatedCode);

            //File.AppendAllLines(@"c:\temp\log.txt", [$"generated={outputPath}"], Encoding.UTF8);
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

    /// <summary>
    /// Determines whether the global options have changed compared to the previously saved state.
    /// </summary>
    /// <remarks>This method checks for changes in the global options by comparing the provided options  with
    /// the options stored in a file located in the output directory. If the file does not  exist or the options have
    /// changed, the method updates the file with the current options.</remarks>
    /// <param name="globalOptions">The current global options to compare against the previously saved options.</param>
    /// <returns><see langword="true"/> if the global options have changed since the last saved state;  otherwise, <see
    /// langword="false"/>.</returns>
    private bool CheckGlobalOptionsHasBeenChanged(GlobalOptions globalOptions)
    {
        var globalOptionsHasBeenChanged = true;
        var prevGlobalOptionsPath = Path.Combine(this.OutputDir, ".globaloptions");
        var prevGlobalOptionsExists = File.Exists(prevGlobalOptionsPath);

        // If the previous global options file exists, read it and compare with the current global options
        if (prevGlobalOptionsExists)
        {
            var entries = new Dictionary<string, string>();
            foreach (var globalOptionLine in File.ReadLines(prevGlobalOptionsPath, Encoding.UTF8))
            {
                var parts = globalOptionLine.Split(['='], 2);
                if (parts.Length != 2) continue;
                entries[parts[0].Trim()] = parts[1].Trim();
            }
            string getValue(string key) => entries.TryGetValue(key, out var value) ? value : string.Empty;
            var prevGlobalOptions = new GlobalOptions(
                rootNamespace: getValue(nameof(this.RootNamespace)),
                projectDir: getValue(nameof(this.ProjectDir)),
                defaultBaseClass: getValue(nameof(this.DefaultBaseClass))
            );
            globalOptionsHasBeenChanged = !globalOptions.Equals(prevGlobalOptions);
        }

        // If the previous global options file does not exist or the options have changed, write the new global options to the file
        if (!prevGlobalOptionsExists || globalOptionsHasBeenChanged)
        {
            File.WriteAllLines(prevGlobalOptionsPath,
            [
                $"{nameof(this.RootNamespace)}={globalOptions.RootNamespace}",
            $"{nameof(this.ProjectDir)}={globalOptions.ProjectDir}",
            $"{nameof(this.DefaultBaseClass)}={globalOptions.DefaultBaseClass}"
            ], Encoding.UTF8);
        }

        //File.AppendAllLines(@"c:\temp\log.txt", ["", $"globalOptionsHasBeenChanged={globalOptionsHasBeenChanged}"], Encoding.UTF8);

        return globalOptionsHasBeenChanged;
    }
}
