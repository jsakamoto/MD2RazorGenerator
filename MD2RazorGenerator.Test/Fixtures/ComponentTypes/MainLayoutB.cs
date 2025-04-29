using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MD2RazorGenerator.Test.Fixtures.ComponentTypes;

public class MainLayoutB : LayoutComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder __builder)
    {
        __builder.OpenElement(0, "main");
        __builder.AddContent(1, this.Body);
        __builder.CloseElement();
    }
}
