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
            var project = await workspace.OpenProjectAsync(@"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\Representer.Exercise.csproj");
            var exercise = project.Documents.Single(document => document.Name == "RepresenterExercise.cs");

            var root = await exercise.GetSyntaxRootAsync();
            var outputPath = Path.Combine(Path.GetDirectoryName(project.FilePath), "representation.txt");

        }
    }
}