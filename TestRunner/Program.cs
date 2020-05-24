using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit.Runners;

namespace TestRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            var projectPath = @"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\TestRunner.Exercise\TestRunner.Exercise.csproj";
            
            // See: https://github.com/microsoft/MSBuildLocator/issues/86
            Process.Start("dotnet", $"restore {projectPath}").WaitForExit();
            
            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projectPath);

            var testsDocument = project.Documents.Single(document => document.Name == "TestRunnerExerciseTests.cs");
            var syntaxRoot = await testsDocument.GetSyntaxRootAsync();
            
            var unskipTests = new UnskipTests();
            testsDocument = testsDocument.WithSyntaxRoot(unskipTests.Visit(syntaxRoot));

            var compilation = await testsDocument.Project.GetCompilationAsync();

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Count(diag => diag.Severity == DiagnosticSeverity.Error);

            var testsFile = @"TestRunner.Exercise.dll";
            var memoryStream = new FileStream(testsFile, FileMode.Create);
            compilation.Emit(memoryStream);
            await memoryStream.DisposeAsync();

            var finished = new ManualResetEventSlim();

            var loadFile = Assembly.LoadFrom(testsFile);

            var assemblyRunner = AssemblyRunner.WithoutAppDomain(loadFile.Location);
            assemblyRunner.OnTestFailed += args => Console.WriteLine($"Test failed: {args.TestDisplayName} - {args.ExceptionMessage}");
            assemblyRunner.OnTestPassed += args => Console.WriteLine($"Test passed: {args.TestDisplayName}");
            assemblyRunner.OnTestSkipped += args => Console.WriteLine($"Test skipped: {args.TestDisplayName}");
            assemblyRunner.OnExecutionComplete += args => finished.Set();
            assemblyRunner.Start();

            finished.Wait();
            finished.Dispose();

            Console.WriteLine($"Diagnostics: {diagnostics.Length}. Errors: {errors}");
        }

        class UnskipTests : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitAttribute(AttributeSyntax node)
            {
                if (node.Name.ToString() == "Fact")
                {
                    return base.VisitAttribute(node.WithArgumentList(null));
                }
                
                return base.VisitAttribute(node);
            }
        }
    }
}