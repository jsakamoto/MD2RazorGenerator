using Mono.Cecil;
using Toolbelt;
using Toolbelt.Diagnostics;

namespace MD2RazorGenerator.Test;

public class BuildTests
{
    public class TestContext : IDisposable
    {
        public string TestProjectDir { get; }
        public WorkDirectory WorkDir { get; }
        public string AssemblyPath { get; }
        public TestContext()
        {
            this.TestProjectDir = FileIO.FindContainerDirToAncestor("MD2RazorGenerator.Test.csproj");
            this.WorkDir = new WorkDirectory(baseDir: Path.Combine([this.TestProjectDir, "bin", "Debug"]));
            this.AssemblyPath = Path.Combine(this.WorkDir, "bin", "Debug", "net8.0", "Project01.dll");
            FileIO.XcopyDir(
                srcDir: Path.Combine([this.TestProjectDir, "Fixtures", "SampleProjects", "Project01"]),
                dstDir: this.WorkDir,
                predicate: arg => arg.Name is not "bin" and not "obj");
        }
        public void Dispose() => this.WorkDir.Dispose();
    }

    [Test]
    public async Task Specify_BaseClass_Test()
    {
        using var testContext = new TestContext();

        using var build = await XProcess.Start("dotnet", "build -p MD2RazorDefaultBaseClass=Project01.CustomComponentBase", testContext.WorkDir).WaitForExitAsync();
        build.ExitCode.Is(0, build.Output);

        using var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath);
        using var module = assembly.MainModule;

        module.GetType("Project01.Welcome")
            .IsNotNull("The type 'Project01.Welcome' was not found in the assembly.")
            .BaseType.IsNotNull("The type 'Project01.Welcome' does not have a base type.")
            .FullName.Is("Project01.CustomComponentBase");
    }

    [Test]
    public async Task Change_BaseClass_from_Default_to_Custom_Test()
    {
        using var testContext = new TestContext();

        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Welcome")
                .IsNotNull("The type 'Project01.Welcome' was not found in the assembly.")
                .BaseType.IsNotNull("The type 'Project01.Welcome' does not have a base type.")
                .FullName.Is("Microsoft.AspNetCore.Components.ComponentBase");
        }

        using (var build = await XProcess.Start("dotnet", "build -p MD2RazorDefaultBaseClass=Project01.CustomComponentBase", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Welcome")
                .IsNotNull("The type 'Project01.Welcome' was not found in the assembly.")
                .BaseType.IsNotNull("The type 'Project01.Welcome' does not have a base type.")
                .FullName.Is("Project01.CustomComponentBase");
        }
    }

    [Test]
    public async Task Change_BaseClass_from_Custom_to_Default_Test()
    {
        using var testContext = new TestContext();

        using (var build = await XProcess.Start("dotnet", "build -p MD2RazorDefaultBaseClass=Project01.CustomComponentBase", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Welcome")
                .IsNotNull("The type 'Project01.Welcome' was not found in the assembly.")
                .BaseType.IsNotNull("The type 'Project01.Welcome' does not have a base type.")
                .FullName.Is("Project01.CustomComponentBase");
        }

        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Welcome")
                .IsNotNull("The type 'Project01.Welcome' was not found in the assembly.")
                .BaseType.IsNotNull("The type 'Project01.Welcome' does not have a base type.")
                .FullName.Is("Microsoft.AspNetCore.Components.ComponentBase");
        }
    }

    [Test]
    public async Task Clean_Test()
    {
        using var testContext = new TestContext();
        var generatedFilePath = Path.Combine(testContext.WorkDir, "obj", "Debug", "net8.0", "md2razor", "Welcome.g.cs");

        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        File.Exists(generatedFilePath).IsTrue("The generated file was not found in the expected location.");

        using (var clean = await XProcess.Start("dotnet", "clean", testContext.WorkDir).WaitForExitAsync())
            clean.ExitCode.Is(0, clean.Output);

        File.Exists(generatedFilePath).IsFalse("The generated file was not deleted after clean.");
    }
}
