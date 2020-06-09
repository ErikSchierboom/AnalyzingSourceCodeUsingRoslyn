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
using Xunit;
using Xunit.Runners;
using Xunit.Sdk;

namespace TestRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var projectFilePath = @"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\TestRunner.Exercise\TestRunner.Exercise.csproj";
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

            var memoryStream = new MemoryStream();
            compilation.Emit(memoryStream);

            var assembly = Assembly.Load(memoryStream.ToArray());
            var assemblyInfo = Reflector.Wrap(assembly);
            var testAssembly = new TestAssembly(assemblyInfo);
            
            var executionMessageSink = new TestMessageSink();
            executionMessageSink.Execution.TestFailedEvent += args => Console.WriteLine($"[FAIL] {args.Message.TestCase.DisplayName}");
            executionMessageSink.Execution.TestSkippedEvent += args => Console.WriteLine($"[SKIP] {args.Message.TestCase.DisplayName}");
            executionMessageSink.Execution.TestPassedEvent += args => Console.WriteLine($"[PASS] {args.Message.TestCase.DisplayName}");

            var discoverySink = new TestDiscoverySink();
            var discoverer = new XunitTestFrameworkDiscoverer(assemblyInfo, new NullSourceInformationProvider(), new TestMessageSink());
            discoverer.Find(false, discoverySink, TestFrameworkOptions.ForDiscovery());
            discoverySink.Finished.WaitOne();

            var testCases = discoverySink.TestCases.Cast<IXunitTestCase>();

            var assemblyRunner = new XunitTestAssemblyRunner(
                testAssembly,
                testCases,
                new TestMessageSink(), 
                executionMessageSink,
                TestFrameworkOptions.ForExecution());
            await assemblyRunner.RunAsync();
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