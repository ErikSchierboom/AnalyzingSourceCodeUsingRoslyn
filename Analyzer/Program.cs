using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace Analyzer
{
    class Program
    {
        static async Task Main()
        {
            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();

            var project = await workspace.OpenProjectAsync(@"C:\Programmeren\AnalyzingSourceCodeUsingRoslyn\Analyzer.Exercise\Analyzer.Exercise.csproj");
            var exercise = project.Documents.Single(document => document.Name == "AnalyzerExercise.cs");

            var root = await exercise.GetSyntaxRootAsync();

            if (UsesMethodOverloading(root))
            {
                Console.WriteLine("Use an optional parameter instead of method overloading");
            }

            if (UsesNullAsDefaultValue(root))
            {
                Console.WriteLine("Use a string instead of null as the default value");
            }

            if (UsesStringConcatenation(root))
            {
                Console.WriteLine("Use string interpolation instead of string concatenation");
            }

            if (CanUseExpressionBody(root))
            {
                Console.WriteLine("Consider using an expression-bodied method");
            }
        }

        private static bool CanUseExpressionBody(SyntaxNode? root)
        {
            var greetingMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(methodDeclaration => methodDeclaration.Identifier.Text == "Greeting");

            return greetingMethod.Body != null && greetingMethod.Body.Statements.Count == 1;
        }

        private static bool UsesStringConcatenation(SyntaxNode? root)
        {
            var greetingMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(methodDeclaration => methodDeclaration.Identifier.Text == "Greeting");

            return greetingMethod
                .DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Any(binaryExpression =>
                    binaryExpression.IsKind(SyntaxKind.AddExpression) &&
                    binaryExpression.Left is LiteralExpressionSyntax left && left.IsKind(SyntaxKind.StringLiteralExpression) ||
                    binaryExpression.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.StringLiteralExpression));
        }

        private static bool UsesNullAsDefaultValue(SyntaxNode? root)
        {
            var greetingMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single(methodDeclaration => methodDeclaration.Identifier.Text == "Greeting");

            return greetingMethod.ParameterList.Parameters[0].Default?.Value is LiteralExpressionSyntax literalExpression &&
                   literalExpression.Token.IsKind(SyntaxKind.NullKeyword);
        }

        private static bool UsesMethodOverloading(SyntaxNode? root)
        {
            return root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Count(methodDeclaration => methodDeclaration.Identifier.Text == "Greeting") > 1;
        }
    }
}