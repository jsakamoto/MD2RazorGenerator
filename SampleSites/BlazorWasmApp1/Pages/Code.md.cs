
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWasmApp1.Pages;

public partial class Code
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Call a Prism to colorize code blocks after the component has rendered
        await this.JsRuntime.InvokeVoidAsync("Prism.highlightAll");
    }
}
