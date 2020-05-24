using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Representer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!MSBuildLocator.IsRegistered)
                MSBuildLocator.RegisterDefaults();
            
            var workspace = MSBuildWorkspace.Create();
            
            var projectFilePath = "/Users/erik/Code/AnalyzingSourceCodeUsingRoslyn/Representer.Exercise/Representer.Exercise.csproj";
            var project = await workspace.OpenProjectAsync(projectFilePath);
            
            var implementation = project.Documents.First(doc => doc.Name == "RepresenterExercise.cs");
            var root = await implementation.GetSyntaxRootAsync();

            var updatedRoot = root;
            updatedRoot = new RemoveUsingStatements().Visit(updatedRoot);
            updatedRoot = new RemoveComments().Visit(updatedRoot);
            updatedRoot = new UseVarDeclarations().Visit(updatedRoot);
            updatedRoot = new AddBracesToIfStatements().Visit(updatedRoot);
            updatedRoot = new NormalizeNumbers().Visit(updatedRoot);
            updatedRoot = new ConditionalWithLiteralFirst().Visit(updatedRoot);
            updatedRoot = new RemoveOptionalElse().Visit(updatedRoot);
            updatedRoot = new SimplifyBooleanClauses().Visit(updatedRoot);
            updatedRoot = new NormalizeIdentifiers().Visit(updatedRoot);
            updatedRoot = new NormalizeWhitespace().Visit(updatedRoot);

            File.WriteAllText("/Users/erik/Code/AnalyzingSourceCodeUsingRoslyn/Representer.Exercise/Representation.txt", updatedRoot.ToFullString());

            Console.WriteLine();
        }

        class NormalizeWhitespace : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
            {
                return base.VisitCompilationUnit(node)?.NormalizeWhitespace();
            }
        }

        class RemoveUsingStatements : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            {
                return null;
            }
        }

        class RemoveComments : CSharpSyntaxRewriter
        {
            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                if (trivia.IsKind(SyntaxKind.XmlComment) ||
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                {
                    return default;
                }
                
                return base.VisitTrivia(trivia);
            }
        }

        class UseVarDeclarations : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                return node.WithType(IdentifierName("var"));
            }
        }
        
        class AddBracesToIfStatements : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
            {
                if (node.Statement is BlockSyntax)
                {
                    return base.VisitIfStatement(node);
                }
                
                return base.VisitIfStatement(node.WithStatement(Block(node.Statement)));
            }

            public override SyntaxNode? VisitElseClause(ElseClauseSyntax node)
            {
                if (node.Statement is BlockSyntax)
                {
                    return base.VisitElseClause(node);
                }
                
                return base.VisitElseClause(node.WithStatement(Block(node.Statement)));
            }
        }
        
        class NormalizeNumbers : CSharpSyntaxRewriter
        {   
            public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.NumericLiteralExpression) &&
                    node.Token.IsKind(SyntaxKind.NumericLiteralToken) &&
                    node.Token.ValueText != node.Token.Text)
                {
                    return base.VisitLiteralExpression(node.WithToken(Literal(node.Token.LeadingTrivia, node.Token.ValueText, node.Token.ValueText, node.Token.TrailingTrivia)));
                }
                
                return base.VisitLiteralExpression(node);
            }
        }
        
        class ConditionalWithLiteralFirst : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.EqualsExpression) &&
                    !(node.Left is LiteralExpressionSyntax) &&
                    node.Right is LiteralExpressionSyntax)
                {
                    return base.VisitBinaryExpression(
                        node.WithLeft(node.Right.WithTriviaFrom(node.Left))
                            .WithRight(node.Left.WithTriviaFrom(node.Right)));
                }
                
                return base.VisitBinaryExpression(node);
            }
        }
        
        class RemoveOptionalElse : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.Body == null)
                {
                    return base.VisitMethodDeclaration(node);
                }

                if (node.Body.Statements.Any() &&
                    node.Body.Statements.Last() is IfStatementSyntax ifStatement &&
                    ifStatement.Else?.Statement is BlockSyntax elseBlock &&
                    elseBlock.Statements.Count == 1 &&
                    elseBlock.Statements.First() is ReturnStatementSyntax returnStatement)
                {
                    return base.VisitMethodDeclaration(
                        node.WithBody(
                            node.Body.WithStatements(
                                node.Body.Statements
                                    .Replace(ifStatement, ifStatement.WithElse(null))
                                    .Add(returnStatement))
                            ));
                }
                
                return base.VisitMethodDeclaration(node);
            }
        }
        
        class SimplifyBooleanClauses : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.EqualsExpression))
                {
                    if (node.Left is LiteralExpressionSyntax literalExpression)
                    {
                        if (literalExpression.IsKind(SyntaxKind.TrueLiteralExpression))
                        {
                            return base.Visit(node.Right);    
                        }
                        
                        if (literalExpression.IsKind(SyntaxKind.FalseLiteralExpression))
                        {
                            return base.Visit(PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, node.Right));
                        }
                    }
                }
                
                return base.VisitBinaryExpression(node);
            }
        }
        
        class NormalizeIdentifiers : CSharpSyntaxRewriter
        {
            private readonly Dictionary<string, string> mappedIdentifiers = new Dictionary<string, string>();
            
            public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                AddMappedIdentifier(node.Identifier);
                return base.VisitClassDeclaration(node.WithIdentifier(GetMappedIdentifier(node.Identifier)));
            }

            public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                AddMappedIdentifier(node.Identifier);
                return base.VisitMethodDeclaration(node.WithIdentifier(GetMappedIdentifier(node.Identifier)));
            }

            public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                AddMappedIdentifier(node.Identifier);
                return base.VisitVariableDeclarator(node.WithIdentifier(GetMappedIdentifier(node.Identifier)));
            }

            public override SyntaxNode? VisitParameter(ParameterSyntax node)
            {
                AddMappedIdentifier(node.Identifier);
                return base.VisitParameter(node.WithIdentifier(GetMappedIdentifier(node.Identifier)));
            }

            public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
            {
                return base.VisitIdentifierName(node.WithIdentifier(GetMappedIdentifier(node.Identifier)));
            }

            private void AddMappedIdentifier(SyntaxToken identifier)
            {
                if (mappedIdentifiers.ContainsKey(identifier.ValueText))
                {
                    return;
                }
                
                mappedIdentifiers.Add(identifier.ValueText, $"IDENTIFIER_{mappedIdentifiers.Count}");
            }

            private SyntaxToken GetMappedIdentifier(SyntaxToken identifier)
            {
                if (mappedIdentifiers.TryGetValue(identifier.ValueText, out var mappedIdentifier))
                {
                    return Identifier(mappedIdentifier);
                }
                
                return identifier;
            }
        }
    }
}