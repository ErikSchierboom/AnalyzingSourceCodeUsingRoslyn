using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Representer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();
            
            var projectFilePath = @"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\Representer.Exercise.csproj";
            var project = await workspace.OpenProjectAsync(projectFilePath);
            
            var implementation = project.Documents.Single(document => document.Name == "RepresenterExercise.cs");

            var root = await implementation.GetSyntaxRootAsync();
            var outputPath = Path.Combine(Path.GetDirectoryName(project.FilePath), "representation.txt");

        }
    }
}