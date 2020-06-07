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

            var workspace = MSBuildWorkspace.Create();

            var project = await workspace.OpenProjectAsync(@"/Users/erik/Code/AnalyzingSourceCodeUsingRoslyn/TestRunner.Exercise/TestRunner.Exercise.csproj");
            Process.Start("dotnet", $"restore {project.FilePath}").WaitForExit(); 
            
            var tests = project.Documents.Single(document => document.Name == "TestRunnerExerciseTests.cs");

            var root = await tests.GetSyntaxRootAsync();
            root = new UnskipTests().Visit(root);
            tests = tests.WithSyntaxRoot(root);

            var compilation = await tests.Project.GetCompilationAsync();
            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            Console.WriteLine($"Diagnostics: {diagnostics.Length}. Errors: {errors}");

            var fileStream = new FileStream(compilation.SourceModule.Name, FileMode.Create);
            compilation.Emit(fileStream);
            fileStream.Close();

            Assembly.LoadFrom(compilation.SourceModule.Name);

            var finished = new ManualResetEventSlim();
            
            Assembly.LoadFrom(compilation.SourceModule.Name);
            
            var runner = AssemblyRunner.WithoutAppDomain(compilation.SourceModule.Name);
            runner.OnTestFailed += info => Console.WriteLine($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}");
            runner.OnTestPassed += info => Console.WriteLine($"[SUCCESS] {info.TestDisplayName}");
            runner.OnTestSkipped += info => Console.WriteLine($"[SKIPPED] {info.TestDisplayName}");
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