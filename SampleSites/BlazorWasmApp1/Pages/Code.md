---
url: /code
title: Code
---

# Code

This component demonstrates how to syntax-highlight code blocks in Razor components coming from markdown files using ["Prism.js."](https://prismjs.com/) See the source code [here](https://github.com/jsakamoto/MD2RazorGenerator/blob/main/SampleSites/BlazorWasmApp1/Pages/Code.md.cs) as well.

HTML

```html
<header>
    <!-- This is a comment. -->
    <h1 id="code">Code</h1>
</header>
```

C#

```csharp
using System;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
app.Run();
```