using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.CodeAnalysis.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class VisualBasicPropertyRegistrationAnalyzer : PropertyRegistrationAnalyzerBase
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyBlock);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var diagnostic = AnalyzeInvocation(context, (InvocationExpressionSyntax)context.Node);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
        {
            var semanticModel = context.SemanticModel;
            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol symbol))
                return null;

            if (!symbol.IsPropertyRegistration(context.Compilation))
                return null;

            var isColumnRegistration = symbol.Name == "RegisterColumn";

            if (!VerifyInvocation(invocationExpression, semanticModel, out var containingType, out var fieldSymbol, out var mounterIdentifier))
                return Diagnostic.Create(Rules.InvalidRegistrationInvocation, invocationExpression.GetLocation());

            var arguments = invocationExpression.ArgumentList.Arguments;
            var firstArgument = arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, containingType, out var propertySymbol, out var nameSyntax))
                return Diagnostic.Create(Rules.InvalidRegistrationGetterParam, firstArgument.GetLocation());

            if (isColumnRegistration && propertySymbol.Type.EqualsTo(KnownTypes.LocalColumnOf, context.Compilation))
                return Diagnostic.Create(Rules.InvalidLocalColumnRegistration, invocationExpression.GetLocation(), propertySymbol.Name);

            if (AnyDuplicate(invocationExpression, propertySymbol, context.Compilation))
                return Diagnostic.Create(Rules.DuplicateRegistration, invocationExpression.GetLocation(), propertySymbol.Name);

            if (fieldSymbol != null)
            {
                var expectedMounterName = "_" + propertySymbol.Name;
                if (fieldSymbol.Name != expectedMounterName)
                    return Diagnostic.Create(Rules.MounterNaming, mounterIdentifier.GetLocation(), fieldSymbol.Name, propertySymbol.Name, expectedMounterName);
            }
            else if (symbol.Name == "Register" && arguments.Count == 2) // Projection column registration
            {
                var fromMounter = semanticModel.GetSymbolInfo(arguments[1].GetExpression()).Symbol as IFieldSymbol;
                if (fromMounter != null && "_" + propertySymbol.Name != fromMounter.Name)
                    return Diagnostic.Create(Rules.ProjectionColumnNaming, nameSyntax.GetLocation(), propertySymbol.Name, fromMounter.Name);
            }
            return null;
        }

        private static bool VerifyInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            if (VerifyStaticFieldInitializer(expression, semanticModel, out fieldSymbol, out mounterIdentifier))
            {
                containingType = fieldSymbol.ContainingType;
                return true;
            }
            return VerifyStaticConstructorInvocation(expression, semanticModel, out containingType, out fieldSymbol, out mounterIdentifier);
        }

        private static bool VerifyStaticFieldInitializer(InvocationExpressionSyntax expression, SemanticModel semanticModel, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            fieldSymbol = null;
            mounterIdentifier = default(SyntaxToken);

            if (!(expression.Parent is EqualsValueSyntax equalsValueClause))
                return false;
            if (!(equalsValueClause.Parent is VariableDeclaratorSyntax variableDeclarator))
                return false;

            Debug.Assert(variableDeclarator.Names.Count == 1);
            var name = variableDeclarator.Names[0];
            if (semanticModel.GetDeclaredSymbol(name) is IFieldSymbol result && result.IsStatic)
            {
                fieldSymbol = result;
                mounterIdentifier = name.Identifier;
                return true;
            }
            return false;
        }

        private static bool VerifyStaticConstructorInvocation(InvocationExpressionSyntax expression, SemanticModel semanticModel, out INamedTypeSymbol containingType, out IFieldSymbol fieldSymbol, out SyntaxToken mounterIdentifier)
        {
            containingType = null;
            fieldSymbol = null;
            mounterIdentifier = default(SyntaxToken);

            ConstructorBlockSyntax constructorBlock = null;
            var assignmentExpression = expression.Parent as AssignmentStatementSyntax;
            if (assignmentExpression != null)
                constructorBlock = assignmentExpression.Parent as ConstructorBlockSyntax;
            else if (expression.Parent is ExpressionStatementSyntax expressionStatement)
                constructorBlock = expressionStatement.Parent as ConstructorBlockSyntax;

            if (constructorBlock == null)
                return false;

            var symbol = semanticModel.GetDeclaredSymbol(constructorBlock);
            if (symbol == null || !symbol.IsStatic)
                return false;
            containingType = symbol.ContainingType;

            if (assignmentExpression != null)
            {
                if (!(assignmentExpression.Left is IdentifierNameSyntax identifierName))
                    return false;

                if (semanticModel.GetSymbolInfo(identifierName).Symbol is IFieldSymbol result && result.IsStatic && result.ContainingType == containingType)
                {
                    fieldSymbol = result;
                    mounterIdentifier = identifierName.Identifier;
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool IsValidGetter(ArgumentSyntax argument, SemanticModel semanticModel, INamedTypeSymbol containingType, out IPropertySymbol propertySymbol, out SimpleNameSyntax nameSyntax)
        {
            propertySymbol = null;
            nameSyntax = null;

            var expression = argument.GetExpression();
            if (!(expression is SingleLineLambdaExpressionSyntax lambdaExpression))
                return false;

            if (!(lambdaExpression.Body is MemberAccessExpressionSyntax memberAccessExpression))
                return false;

            if (!memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return false;

            if (!(semanticModel.GetSymbolInfo(memberAccessExpression).Symbol is IPropertySymbol result))
                return false;

            if (result.ContainingType != containingType)
                return false;

            if (result.SetMethod == null)
                return false;

            propertySymbol = result;
            nameSyntax = memberAccessExpression.Name;
            return true;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, Compilation compilation)
        {
            var containingType = propertySymbol.ContainingType;
            var syntaxReferences = containingType.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var classStatement = (ClassStatementSyntax)syntaxReferences[i].GetSyntax();
                var classBlock = (ClassBlockSyntax)classStatement.Parent;
                var semanticModel = compilation.GetSemanticModel(classBlock.SyntaxTree);
                if (AnyDuplicate(invocationExpression, propertySymbol, classBlock, semanticModel))
                    return true;
            }

            return false;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, ClassBlockSyntax classBlock, SemanticModel semanticModel)
        {
            var result = GetPropertyRegistration(propertySymbol, classBlock, semanticModel);
            return result == null ? false : CompareLocation(invocationExpression, result) > 0;
        }

        private static bool AnyDuplicate(InvocationExpressionSyntax invocationExpression, IPropertySymbol propertySymbol, ConstructorBlockSyntax constructorBlock, SemanticModel semanticModel)
        {
            var result = GetPropertyRegistration(propertySymbol, constructorBlock, semanticModel);
            return result == null ? false : CompareLocation(invocationExpression, result) > 0;
        }

        private static int CompareLocation(InvocationExpressionSyntax x, InvocationExpressionSyntax y)
        {
            if (x == y)
                return 0;

            var result = Comparer<string>.Default.Compare(x.SyntaxTree.FilePath, y.SyntaxTree.FilePath);
            if (result != 0)
                return result;
            return Comparer<int>.Default.Compare(x.GetLocation().SourceSpan.Start, y.GetLocation().SourceSpan.Start);
        }

        private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyBlock = (PropertyBlockSyntax)context.Node;
            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyBlock);

            var diagnostic = AnalyzeProperty(propertyBlock.PropertyStatement.Identifier, propertySymbol, context.Compilation);
            if (diagnostic != null)
                context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic AnalyzeProperty(SyntaxToken identifier, IPropertySymbol propertySymbol, Compilation compilation)
        {
            if (propertySymbol.SetMethod == null)
                return null;

            if (!propertySymbol.GetPropertyRegistrationType(compilation).HasValue)
                return null;

            var syntaxReferences = propertySymbol.ContainingType.DeclaringSyntaxReferences;
            for (int i = 0; i < syntaxReferences.Length; i++)
            {
                var classStatement = (ClassStatementSyntax)syntaxReferences[i].GetSyntax();
                var classBlock = (ClassBlockSyntax)classStatement.Parent;
                var semanticModel = compilation.GetSemanticModel(classBlock.SyntaxTree);
                if (GetPropertyRegistration(propertySymbol, classBlock, semanticModel) != null)
                    return null;
            }

            return Diagnostic.Create(Rules.MissingRegistration, identifier.GetLocation(), propertySymbol.Name);
        }

        private static InvocationExpressionSyntax GetPropertyRegistration(IPropertySymbol propertySymbol, ClassBlockSyntax classBlock, SemanticModel semanticModel)
        {
            var members = classBlock.Members;
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member is ConstructorBlockSyntax constructorBlock)
                {
                    var result = GetPropertyRegistration(propertySymbol, constructorBlock, semanticModel);
                    if (result != null)
                        return result;
                }
                if (member is FieldDeclarationSyntax fieldDeclaration)
                {
                    var result = GetPropertyRegistration(propertySymbol, fieldDeclaration, semanticModel);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        private static InvocationExpressionSyntax GetPropertyRegistration(IPropertySymbol propertySymbol, ConstructorBlockSyntax constructorBlock, SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetDeclaredSymbol(constructorBlock);
            if (symbol == null || !symbol.IsStatic)
                return null;

            var statements = constructorBlock.Statements;
            for (int i = 0; i < statements.Count; i++)
            {
                var statement = statements[i];
                if (statement is AssignmentStatementSyntax assignmentStatement)
                {
                    if (assignmentStatement.Right is InvocationExpressionSyntax invocationExpression && IsPropertyRegistration(invocationExpression, propertySymbol, semanticModel))
                        return invocationExpression;
                }
                else if (statement is ExpressionStatementSyntax expressionStatement)
                {
                    var expression = expressionStatement.Expression;
                    if (expression is InvocationExpressionSyntax invocationExpression && IsPropertyRegistration(invocationExpression, propertySymbol, semanticModel))
                        return invocationExpression;
                }
            }

            return null;
        }

        private static InvocationExpressionSyntax GetPropertyRegistration(IPropertySymbol propertySymbol, FieldDeclarationSyntax fieldDeclaration, SemanticModel semanticModel)
        {
            var declarators = fieldDeclaration.Declarators;
            if (declarators.Count != 1)
                return null;
            var variableDeclarator = declarators[0];
            var initializer = variableDeclarator.Initializer;
            if (initializer == null)
                return null;
            Debug.Assert(variableDeclarator.Names.Count == 1);
            if (!(semanticModel.GetDeclaredSymbol(variableDeclarator.Names[0]) is IFieldSymbol fieldSymbol) || !fieldSymbol.IsStatic)
                return null;

            var result = initializer.Value as InvocationExpressionSyntax;
            return IsPropertyRegistration(result, propertySymbol, semanticModel) ? result : null;
        }

        private static bool IsPropertyRegistration(InvocationExpressionSyntax expression, IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            if (!(semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol symbol))
                return false;

            if (!symbol.IsPropertyRegistration(semanticModel.Compilation))
                return false;

            var firstArgument = expression.ArgumentList.Arguments[0];
            Debug.Assert(firstArgument != null);
            if (!IsValidGetter(firstArgument, semanticModel, propertySymbol.ContainingType, out var otherPropertySymbol, out _))
                return false;

            return propertySymbol.Name == otherPropertySymbol.Name;
        }
    }
}
