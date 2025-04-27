
namespace Toolbelt.Blazor.MD2Razor.Internals;

internal class FrontMatter
{
    public IEnumerable<string> Pages { get; }
    public IEnumerable<string> Usings { get; }
    public string? Namespace { get; }
    public IEnumerable<string> Attributes { get; }
    public string? Inherit { get; }
    public string? PageTitle { get; }

    public FrontMatter() : this([], [], [], [], [], [])
    {
    }

    public FrontMatter(IEnumerable<string> pages, IEnumerable<string> usings, IEnumerable<string> namespaces, IEnumerable<string> attributes, IEnumerable<string> inherits, IEnumerable<string> pageTitles)
    {
        this.Pages = pages;
        this.Usings = usings;
        this.Namespace = namespaces.FirstOrDefault();
        this.Attributes = attributes;
        this.Inherit = inherits.FirstOrDefault();
        this.PageTitle = pageTitles.FirstOrDefault();
    }
}
