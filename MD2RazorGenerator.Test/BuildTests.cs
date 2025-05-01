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
        // Given
        using var testContext = new TestContext();
        var generatedFilePath = Path.Combine(testContext.WorkDir, "obj", "Debug", "net8.0", "md2razor", "Welcome.md.g.cs");

        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        File.Exists(generatedFilePath).IsTrue("The generated file should be generated after build, but it was not.");

        // When
        using (var clean = await XProcess.Start("dotnet", "clean", testContext.WorkDir).WaitForExitAsync())
            clean.ExitCode.Is(0, clean.Output);

        // Then
        File.Exists(generatedFilePath).IsFalse("The generated file should be deleted after clean, but it was not.");
    }

    [Test]
    public async Task Move_FileLocation_Test()
    {
        // Given
        using var testContext = new TestContext();
        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Welcome").IsNotNull("The type 'Project01.Welcome' was not found in the assembly.");
            module.GetType("Project01.Pages.Welcome").IsNull("The type 'Project01.Pages.Welcome' should not be found in the assembly, but it was.");
        }

        // When
        var oldPathOfWelcomeMd = Path.Combine(testContext.WorkDir, "Welcome.md");
        var newPathOfWelcomeMd = Path.Combine(testContext.WorkDir, "Pages", "Welcome.md");
        File.Move(oldPathOfWelcomeMd, newPathOfWelcomeMd);

        using (var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync())
            build.ExitCode.Is(0, build.Output);

        // Then
        using (var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath))
        {
            using var module = assembly.MainModule;
            module.GetType("Project01.Pages.Welcome").IsNotNull("The type 'Project01.Pages.Welcome' was not found in the assembly.");
            module.GetType("Project01.Welcome").IsNull("The type 'Project01.Welcome' should not be found in the assembly, but it was.");
        }
    }

    [Test]
    public async Task Has_Markdowns_with_SameName_in_DifferentDir_Test()
    {
        // Given
        using var testContext = new TestContext();
        Directory.CreateDirectory(Path.Combine(testContext.WorkDir, "AnotherDir"));
        File.Copy(
            Path.Combine(testContext.WorkDir, "Welcome.md"),
            Path.Combine(testContext.WorkDir, "AnotherDir", "Welcome.md"));

        // When
        using var build = await XProcess.Start("dotnet", "build", testContext.WorkDir).WaitForExitAsync();
        build.ExitCode.Is(0, build.Output);

        // Then: There should be two types with the same name in different namespaces
        using var assembly = AssemblyDefinition.ReadAssembly(testContext.AssemblyPath);
        using var module = assembly.MainModule;
        module.GetType("Project01.Welcome").IsNotNull("The type 'Project01.Welcome' should exist in the assembly, but it was not found.");
        module.GetType("Project01.AnotherDir.Welcome").IsNotNull("The type 'Project01.AnotherDir.Welcome' should exist in the assembly, but it was not found.");
    }
}
