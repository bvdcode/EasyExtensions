using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ExplicitLocalVariableTypeAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.ExplicitLocalVariableType];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
			context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
		}

		private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
		{
			VariableDeclarationSyntax declaration = (VariableDeclarationSyntax)context.Node;

			if (!declaration.Type.IsVar)
			{
				return;
			}

			bool allInitializersAllowed = declaration.Variables.Count > 0 &&
				declaration.Variables.All(variable =>
					variable.Initializer is not null &&
					IsAllowedInitializer(variable.Initializer.Value, context));

			if (!allInitializersAllowed)
			{
				Report(context, declaration.Type.GetLocation());
			}
		}

		private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
		{
			ForEachStatementSyntax statement = (ForEachStatementSyntax)context.Node;

			if (statement.Type.IsVar && !HasAnonymousElementType(statement, context))
			{
				Report(context, statement.Type.GetLocation());
			}
		}

		private static void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
		{
			DeclarationExpressionSyntax declaration = (DeclarationExpressionSyntax)context.Node;

			if (declaration.Type.IsVar &&
				!IsTupleDeconstruction(declaration) &&
				!HasAnonymousDeclaredType(declaration, context))
			{
				Report(context, declaration.Type.GetLocation());
			}
		}

		private static bool IsAllowedInitializer(ExpressionSyntax initializer, SyntaxNodeAnalysisContext context)
		{
			return initializer is QueryExpressionSyntax ||
				initializer is AnonymousObjectCreationExpressionSyntax ||
				IsGenericObjectCreation(initializer) ||
				IsLinqExpression(initializer, context);
		}

		private static bool IsGenericObjectCreation(ExpressionSyntax initializer)
		{
			return initializer is ObjectCreationExpressionSyntax creation &&
				creation.Type.DescendantNodesAndSelf().OfType<GenericNameSyntax>().Any();
		}

		private static bool IsLinqExpression(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
		{
			if (expression is AwaitExpressionSyntax awaitExpression)
			{
				return IsLinqExpression(awaitExpression.Expression, context);
			}

			if (expression is ParenthesizedExpressionSyntax parenthesizedExpression)
			{
				return IsLinqExpression(parenthesizedExpression.Expression, context);
			}

			if (expression is QueryExpressionSyntax)
			{
				return true;
			}

			if (expression is InvocationExpressionSyntax invocation &&
				(IsLinqInvocation(invocation, context) ||
				invocation.DescendantNodes()
					.OfType<InvocationExpressionSyntax>()
					.Any(childInvocation => IsLinqInvocation(childInvocation, context))))
			{
				return true;
			}

			ITypeSymbol? expressionType = context.SemanticModel
				.GetTypeInfo(expression, context.CancellationToken)
				.Type;
			return IsQueryableType(expressionType);
		}

		private static bool IsLinqInvocation(
			InvocationExpressionSyntax invocation,
			SyntaxNodeAnalysisContext context)
		{
			IMethodSymbol? method = context.SemanticModel
				.GetSymbolInfo(invocation, context.CancellationToken)
				.Symbol as IMethodSymbol;
			IMethodSymbol? originalMethod = method?.ReducedFrom ?? method;

			if (originalMethod is null)
			{
				return false;
			}

			string namespaceName = originalMethod.ContainingNamespace.ToDisplayString();

			if (namespaceName == "System.Linq")
			{
				return true;
			}

			return namespaceName.StartsWith("Microsoft.EntityFrameworkCore", System.StringComparison.Ordinal) &&
				originalMethod.IsExtensionMethod &&
				originalMethod.Parameters.Length > 0 &&
				IsQueryableType(originalMethod.Parameters[0].Type);
		}

		private static bool IsQueryableType(ITypeSymbol? type)
		{
			if (type is not INamedTypeSymbol namedType)
			{
				return false;
			}

			return new[] { namedType }
				.Concat(namedType.AllInterfaces)
				.Any(candidate =>
					candidate.Name == "IQueryable" &&
					candidate.ContainingNamespace.ToDisplayString() == "System.Linq");
		}

		private static bool IsTupleDeconstruction(DeclarationExpressionSyntax declaration)
		{
			if (declaration.Designation is ParenthesizedVariableDesignationSyntax)
			{
				return true;
			}

			return declaration.Ancestors()
				.OfType<AssignmentExpressionSyntax>()
				.Any(assignment => assignment.Left.Span.Contains(declaration.Span));
		}

		private static bool HasAnonymousElementType(
			ForEachStatementSyntax statement,
			SyntaxNodeAnalysisContext context)
		{
			ITypeSymbol? elementType = context.SemanticModel
				.GetForEachStatementInfo(statement)
				.ElementType;
			return IsAnonymousType(elementType);
		}

		private static bool HasAnonymousDeclaredType(
			DeclarationExpressionSyntax declaration,
			SyntaxNodeAnalysisContext context)
		{
			if (declaration.Designation is not SingleVariableDesignationSyntax designation)
			{
				return false;
			}

			ILocalSymbol? local = context.SemanticModel
				.GetDeclaredSymbol(designation, context.CancellationToken) as ILocalSymbol;
			return IsAnonymousType(local?.Type);
		}

		private static bool IsAnonymousType(ITypeSymbol? type)
		{
			return type is INamedTypeSymbol namedType && namedType.IsAnonymousType;
		}

		private static void Report(SyntaxNodeAnalysisContext context, Location location)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.ExplicitLocalVariableType,
				location));
		}
	}
}
