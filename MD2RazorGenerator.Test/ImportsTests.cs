using Toolbelt.Blazor.MD2Razor.Internals;

namespace MD2RazorGenerator.Test;

public class ImportsTests
{
    [Test]
    public void GetUsings_Test()
    {
        var imports = new Imports(path: "_Imports.razor", """
            @using System

            @using System.Collections.Generic @* Comment *@
            @using System.Linq
            """);

        imports.GetUsings().Is(
            "System",
            "System.Collections.Generic",
            "System.Linq");
    }

    [Test]
    public void Applicable_Test()
    {
        var importsCollection = new Imports[] {
            new(path:"C:\\Project\\_Imports.razor", text:""),
            new(path:"C:\\Project\\Foo\\_Imports.razor", text:""),
            new(path:"C:\\Project\\Foo\\Bar\\_Imports.razor", text:""),
            new(path:"C:\\Project\\Foo\\Fizz\\_Imports.razor", text:""),
            new(path:"C:\\Project\\Buzz\\_Imports.razor", text:""),
        };

        var applicableImports = importsCollection.GetApplicableImports("C:\\Project\\Foo\\Bar\\Welcome.md");
        applicableImports.Select(i => i.Path).Is(
            "C:\\Project\\_Imports.razor",
            "C:\\Project\\Foo\\_Imports.razor",
            "C:\\Project\\Foo\\Bar\\_Imports.razor"
        );
    }
}
