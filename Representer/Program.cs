using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Representer
{
    class Program
    {
        static void Main(string[] args)
        {
            var exerciseFilePath = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\RepresenterExercise.cs";
            var representationFile = @"C:\Code\AnalyzingSourceCodeUsingRoslyn\Representer.Exercise\representation.txt";

            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(exerciseFilePath));
            var representation = tree.GetRoot();

            representation = new RemoveUsingStatements().Visit(representation);
            representation = new RemoveComments().Visit(representation);
            representation = new UseVarDeclarations().Visit(representation);
            representation = new SimplifyBooleanExpressions().Visit(representation);
            representation = new NormalizeNumbers().Visit(representation);
            representation = new PreferLiteralOnLeftSide().Visit(representation);
            representation = new AddBracesToIfOrElse().Visit(representation);
            representation = new NormalizeIdentifiers().Visit(representation);

            File.WriteAllText(representationFile, representation.NormalizeWhitespace().ToFullString());
            
            Console.WriteLine("Hello from the Representer!");
        }
    }

    class NormalizeIdentifiers : CSharpSyntaxRewriter
    {
        private Dictionary<string, string> identifierMapping = new Dictionary<string, string>();
        
        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            AddMappedIdentifier(node.Identifier);
            return base.VisitClassDeclaration(node.WithIdentifier(MappedIdentifier(node.Identifier)));
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddMappedIdentifier(node.Identifier);
            return base.VisitMethodDeclaration(node.WithIdentifier(MappedIdentifier(node.Identifier)));
        }

        public override SyntaxNode? VisitParameter(ParameterSyntax node)
        {
            AddMappedIdentifier(node.Identifier);
            return base.VisitParameter(node.WithIdentifier(MappedIdentifier(node.Identifier)));
        }

        public override SyntaxNode? VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            AddMappedIdentifier(node.Identifier);
            return base.VisitVariableDeclarator(node.WithIdentifier(MappedIdentifier(node.Identifier)));
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            return base.VisitIdentifierName(node.WithIdentifier(MappedIdentifier(node.Identifier)));
        }

        private void AddMappedIdentifier(SyntaxToken identifier)
        {
            identifierMapping.TryAdd(identifier.Text, $"IDENTIFIER_{identifierMapping.Count}");
        }

        private SyntaxToken MappedIdentifier(SyntaxToken identifier)
        {
            if (identifierMapping.TryGetValue(identifier.Text, out var mappedIdentifier))
            {
                return SyntaxFactory.Identifier(mappedIdentifier);
            }

            return identifier;
        }
    }

    class AddBracesToIfOrElse : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Statement is BlockSyntax)
            {
                return base.VisitIfStatement(node);
            }
            
            return base.VisitIfStatement(node.WithStatement(SyntaxFactory.Block(node.Statement)));
        }

        public override SyntaxNode? VisitElseClause(ElseClauseSyntax node)
        {
            if (node.Statement is BlockSyntax)
            {
                return base.VisitElseClause(node);
            }
            
            return base.VisitElseClause(node.WithStatement(SyntaxFactory.Block(node.Statement)));
        }
    }

    class PreferLiteralOnLeftSide : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.EqualsExpression) &&
                node.Right is LiteralExpressionSyntax && 
                !(node.Left is LiteralExpressionSyntax))
            {
                return base.VisitBinaryExpression(node.WithLeft(node.Right).WithRight(node.Left));
            }
            
            return base.VisitBinaryExpression(node);
        }
    }

    class NormalizeNumbers : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return base.VisitLiteralExpression(
                    node.WithToken(
                        SyntaxFactory.Literal(
                            node.Token.LeadingTrivia, node.Token.ValueText, node.Token.ValueText, node.Token.TrailingTrivia)));
            }
            
            return base.VisitLiteralExpression(node);
        }
    }
    
    class SimplifyBooleanExpressions : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.EqualsExpression) &&
                node.Right is LiteralExpressionSyntax right)
            {
                if (right.IsKind(SyntaxKind.TrueLiteralExpression))
                {
                    return base.Visit(node.Left);    
                }

                if (right.IsKind(SyntaxKind.FalseLiteralExpression))
                {
                    return base.Visit(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, node.Left));
                }
            }
            
            return base.VisitBinaryExpression(node);
        }
    }

    class UseVarDeclarations : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            if (node.Type.IsVar)
            {
                return base.VisitVariableDeclaration(node);    
            }

            return base.VisitVariableDeclaration(node.WithType(SyntaxFactory.IdentifierName("var")));
        }
    }

    class RemoveComments : CSharpSyntaxRewriter
    {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
            {
                return default;
            }
            
            return base.VisitTrivia(trivia);
        }
    }

    class RemoveUsingStatements : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            return null;
        }
    }
}