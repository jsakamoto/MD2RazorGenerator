namespace Toolbelt.Blazor.MD2Razor.Internals;

internal class GlobalOptions : IEquatable<GlobalOptions?>
{
    public string RootNamespace { get; }

    public string ProjectDir { get; }

    public string DefaultBaseClass { get; }

    public GlobalOptions(string rootNamespace, string projectDir, string defaultBaseClass)
    {
        this.RootNamespace = rootNamespace;
        this.ProjectDir = projectDir;
        this.DefaultBaseClass = string.IsNullOrWhiteSpace(defaultBaseClass) ? "global::Microsoft.AspNetCore.Components.ComponentBase" : defaultBaseClass;
    }

    public override bool Equals(object? obj) => this.Equals(obj as GlobalOptions);

    public bool Equals(GlobalOptions? other)
    {
        return other is not null &&
            this.RootNamespace == other.RootNamespace &&
            this.ProjectDir == other.ProjectDir &&
            this.DefaultBaseClass == other.DefaultBaseClass;
    }

    public override int GetHashCode()
    {
        var hashCode = -847805451;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.RootNamespace);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.ProjectDir);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.DefaultBaseClass);
        return hashCode;
    }
}
