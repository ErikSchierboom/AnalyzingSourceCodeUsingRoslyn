using System;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace TestRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();
            
            var projectFilePath = @"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\TestRunner.Exercise\TestRunner.Exercise.csproj";
            var project = await workspace.OpenProjectAsync(projectFilePath);
            
            Console.WriteLine("Hello from the Test Runner!");
        }
    }
}