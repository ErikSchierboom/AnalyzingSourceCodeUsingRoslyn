using System;
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

            var representation = await exercise.GetSyntaxRootAsync();
            
            Console.WriteLine("Hello from the Representer!");
        }
    }
}