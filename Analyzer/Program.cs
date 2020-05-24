using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace Analyzer
{
    class Program
    {
        static async Task Main()
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            var workspace = MSBuildWorkspace.Create();
            
            var projectFilePath = "/Users/erik/Code/AnalyzingSourceCodeUsingRoslyn/Analyzer.Exercise/Analyzer.Exercise.csproj";
            var project = await workspace.OpenProjectAsync(projectFilePath);

            var implementation = project.Documents.First(doc => doc.Name == "AnalyzerExercise.cs");
            var root = await implementation.GetSyntaxRootAsync();

            if (UsesMethodOverloading(root))
            {
                Console.WriteLine($"You can use an optional parameter to replace the method overloading.");
                return;
            }
            
            var greetingMethod = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First(method => method.Identifier.Text == "Greeting");

            if (UsesNullAsDefaultValue(greetingMethod))
            {
                Console.WriteLine($"You can use a string value as the default value for the {greetingMethod.ParameterList.Parameters[0].Identifier.Text} parameter.");
                return;
            }

            if (UsesStringConcatenation(greetingMethod))
            {
                Console.WriteLine($"You can use a string interpolation instead of string concatenation to format the string.");
                return;
            }

            if (BodyHasSingleStatement(greetingMethod))
            {
                Console.WriteLine($"You can write the {greetingMethod.Identifier.Text}() method as an expression body");
                return;
            }

            Console.WriteLine("Well done!");
        }

        private static bool UsesMethodOverloading(SyntaxNode root)
        {
            return root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Count(method => method.Identifier.Text == "Greeting") > 1;
        }

        private static bool UsesNullAsDefaultValue(MethodDeclarationSyntax greetingMethod)
        {
            return greetingMethod.ParameterList.Parameters[0].Default is EqualsValueClauseSyntax equalsValueClause &&
                   equalsValueClause.Value is LiteralExpressionSyntax literalExpression &&
                   literalExpression.Token.IsKind(SyntaxKind.NullKeyword);
        }

        private static bool UsesStringConcatenation(MethodDeclarationSyntax greetingMethod)
        {
            return greetingMethod.DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Where(binaryExpression => binaryExpression.IsKind(SyntaxKind.AddExpression))
                .Any(binaryExpression =>
                    binaryExpression.Left is LiteralExpressionSyntax left && left.IsKind(SyntaxKind.StringLiteralExpression) ||
                    binaryExpression.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.StringLiteralExpression));
        }

        private static bool BodyHasSingleStatement(MethodDeclarationSyntax greetingMethod)
        {
            return greetingMethod.Body != null &&
                   greetingMethod.Body.Statements.Count == 1;
        }
    }
}