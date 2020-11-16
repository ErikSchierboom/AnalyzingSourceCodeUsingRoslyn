﻿using System.IO;
using Microsoft.CodeAnalysis.CSharp;

namespace Representer
{
    class Program
    {
        static void Main()
        {
            var implementationFilePath = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\RepresenterExercise.cs";
            var implementation = File.ReadAllText(implementationFilePath);

            var tree = CSharpSyntaxTree.ParseText(implementation);
            var root = tree.GetRoot();
            
            // var representationFile = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\representation.txt";
            // TODO: write representation to file
        }
    }
}