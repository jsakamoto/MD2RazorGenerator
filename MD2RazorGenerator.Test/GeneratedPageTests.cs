using Bunit;
using Fizz.Buzz.FizzBuzz;
using Lorem.Ipsum;
using MD2RazorGenerator.Test.Fixtures;
using MD2RazorGenerator.Test.Fixtures.Attributes;
using MD2RazorGenerator.Test.Fixtures.Attributes.FizzBuzz;
using MD2RazorGenerator.Test.Fixtures.ComponentTypes;
using MD2RazorGenerator.Test.Fixtures.ComponentTypes.OtherLayouts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MD2RazorGenerator.Test;

public class GeneratedPageTests
{
    [Test]
    public void NoYamlFrontMatter_Test()
    {
        // No Attributes
        var attributes = typeof(Sample01_NoYamlFrontMatter).GetCustomAttributes(inherit: true);
        attributes.Length.Is(0, "Expected No attribute, but found {0}");

        // Default namespace based on file path
        typeof(Sample01_NoYamlFrontMatter).Namespace.Is("MD2RazorGenerator.Test.Fixtures");

        // Default base class
        typeof(Sample01_NoYamlFrontMatter).BaseType.Is(typeof(ComponentBase));
    }

    [Test]
    public void WithYamlFrontMatter_Scalar_Test()
    {
        // A [Foo], a [Route], and a [Layout] attribute in the front matter
        var attributes = typeof(Sample02_WithYamlFrontMatter_Scalar).GetCustomAttributes(inherit: true);
        attributes.Length.Is(3, "Expected 3 attribute, but found {0}");
        attributes.OfType<FooAttribute>().Single().Text.Is("Hello", "Expected FooAttribute with 'Hello', but not found");
        attributes.OfType<RouteAttribute>().Single().Template.Is("/sample02", "Expected RouteAttribute with '/sample02', but not found");
        attributes.OfType<LayoutAttribute>().Single().LayoutType.Is(typeof(MainLayoutA));

        // The namespace specified in the front matter
        typeof(Sample02_WithYamlFrontMatter_Scalar).Namespace.Is("Lorem.Ipsum");

        // The base class specified in the front matter
        typeof(Sample02_WithYamlFrontMatter_Scalar).BaseType.Is(typeof(MarkdownComponentBaseA));
    }

    [Test]
    public void WithYamlFrontMatter_Sequence_Test()
    {
        // A [Foo], a [Bar], three [Route], and a [Layout] attributes in the front matter
        var attributes = typeof(Sample03_WithYamlFrontMatter_Sequence).GetCustomAttributes(inherit: true);
        attributes.Length.Is(6, "Expected 6 attribute, but found {0}");
        attributes.OfType<FooAttribute>().Single().Text.Is("Hello", "Expected FooAttribute with 'Hello', but not found");
        attributes.OfType<BarAttribute>().Single().Text.Is("World", "Expected FooAttribute with 'World', but not found");
        attributes.OfType<RouteAttribute>().Select(a => a.Template).Order().Is(
            "/sample03",
            "/sample03/A",
            "/sample03/B");
        attributes.OfType<LayoutAttribute>().Single().LayoutType.Is(typeof(MainLayoutB));

        // The namespace specified the first in the sequence
        typeof(Sample03_WithYamlFrontMatter_Sequence).Namespace.Is("Fizz.Buzz.FizzBuzz");

        // The base class specified the first in the sequence
        typeof(Sample03_WithYamlFrontMatter_Sequence).BaseType.Is(typeof(MarkdownComponentBaseB));
    }

    [Test]
    public void HyperLinks_Test()
    {
        using var context = new Bunit.TestContext();
        using var cut = context.RenderComponent<Sample04_HyperLinks>();

        // If the URL is absolute, it should be opened in a new tab
        cut.MarkupMatches("""
            <ul>
                <li><a href="https://blazor.net/" target="_blank">About Blazor</a></li>
                <li><a href="https://blazor.net/" target="_blank">https://blazor.net/</a></li>
                <li><a href="/home">Home</a></li>
                <li><a href="./counter">Counter</a></li>
            </ul>
            """);
    }

    [Test]
    public void Title_in_Scaler_Test()
    {
        using var context = new Bunit.TestContext();
        using var cut = context.RenderComponent<Sample02_WithYamlFrontMatter_Scalar>();

        using var pageTitleComponent = cut.FindComponent<PageTitle>();
        pageTitleComponent.IsNotNull("Expected PageTitle component, but not found");
        pageTitleComponent.Instance.ChildContent.IsNotNull("Page title content should be specified, but did'nt.");
        using var pageTitleText = context.Render(pageTitleComponent.Instance.ChildContent);
        pageTitleText.Markup.Is("Sample 02");
    }

    [Test]
    public void Title_in_Sequence_Test()
    {
        using var context = new Bunit.TestContext();
        using var cut = context.RenderComponent<Sample03_WithYamlFrontMatter_Sequence>();

        using var pageTitleComponent = cut.FindComponent<PageTitle>();
        pageTitleComponent.IsNotNull("Expected PageTitle component, but not found");
        pageTitleComponent.Instance.ChildContent.IsNotNull("Page title content should be specified, but did'nt.");
        using var pageTitleText = context.Render(pageTitleComponent.Instance.ChildContent);
        pageTitleText.Markup.Is("Sample 03");
    }

    [Test]
    public void DependsOn_ImportsRazor_Test()
    {
        // A [FizzBuzz] and a [Layout] attribute in the front matter
        var attributes = typeof(Sample06_DependsImports).GetCustomAttributes(inherit: true);
        attributes.Length.Is(2, "Expected 2 attribute, but found {0}");
        attributes.OfType<FizzBuzzAttribute>().Single().Text.Is("Hello, World!", "Expected FizzBuzzAttribute with 'Hello, World!', but not found");
        attributes.OfType<LayoutAttribute>().Single().LayoutType.Is(typeof(MainLayoutC));
    }

    [Test]
    public void Table_Test()
    {
        using var context = new Bunit.TestContext();
        using var cut = context.RenderComponent<Sample05_Table>();
        cut.MarkupMatches("""
            <table>
                <thead>
                    <tr>
                        <th>Column A</th>
                        <th>Column B</th>
                        <th>Column C</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Lorem</td>
                        <td>Ipsum</td>
                        <td>Dolor</td>
                    </tr>
                    <tr>
                        <td>Sit</td>
                        <td>Amet</td>
                        <td>Consecte</td>
                    </tr>
                </tbody>
            </table>
            """);
    }
}
