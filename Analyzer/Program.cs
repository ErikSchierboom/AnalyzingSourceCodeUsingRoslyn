using System;
using System.IO;

namespace Analyzer
{
    class Program
    {
        static void Main()
        {
            var implementationFilePath = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\Analyzer.Exercise\AnalyzerExercise.cs";
            var implementation = File.ReadAllText(implementationFilePath);
            
            Console.WriteLine("Hello from the Analyzer!");
        }
    }
}