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
            MSBuildLocator.RegisterDefaults();

            var projectFilePath = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\TestRunner.Exercise\TestRunner.Exercise.csproj";
            Process.Start("dotnet", $"restore {projectFilePath}").WaitForExit();

            var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(projectFilePath);
            
            var tests = project.Documents.Single(document => document.Name == "TestRunnerExerciseTests.cs");
            var root = await tests.GetSyntaxRootAsync();
            root = new UnskipTests().Visit(root);
            tests = tests.WithSyntaxRoot(root);

            var compilation = await tests.Project.GetCompilationAsync();
            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            Console.WriteLine($"Diagnostics: {diagnostics.Length}. Errors: {errors}");

            compilation.Emit(compilation.SourceModule.Name);

            Assembly.LoadFrom(compilation.SourceModule.Name);
            
            var finished = new ManualResetEventSlim();
            var runner = AssemblyRunner.WithoutAppDomain(compilation.SourceModule.Name);
            runner.OnTestFailed += info => Console.WriteLine($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}");
            runner.OnTestPassed += info => Console.WriteLine($"[PASS] {info.TestDisplayName}");
            runner.OnTestSkipped += info => Console.WriteLine($"[SKIP] {info.TestDisplayName}");
            runner.OnExecutionComplete += info => finished.Set();
            
            runner.Start();
            finished.Wait();
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