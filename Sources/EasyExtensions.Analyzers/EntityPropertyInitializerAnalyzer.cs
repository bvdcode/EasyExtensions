using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityPropertyInitializerAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EntityPropertyInitializer];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
		}

		private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
		{
			PropertyDeclarationSyntax declaration = (PropertyDeclarationSyntax)context.Node;
			IPropertySymbol? property = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);

			if (property is null ||
				property.IsStatic ||
				declaration.Initializer is null ||
				EntityModel.IsNotMapped(property) ||
				!SymbolHelpers.IsEntity(property.ContainingType))
			{
				return;
			}

			if (!IsAllowed(
				property,
				declaration.Initializer.Value,
				context.SemanticModel,
				context.CancellationToken))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.EntityPropertyInitializer,
					declaration.Initializer.GetLocation(),
					property.Name));
			}
		}

		private static bool IsAllowed(
			IPropertySymbol property,
			ExpressionSyntax initializer,
			SemanticModel semanticModel,
			System.Threading.CancellationToken cancellationToken)
		{
			if (property.NullableAnnotation != NullableAnnotation.NotAnnotated)
			{
				return false;
			}

			if (property.Type.SpecialType == SpecialType.System_String)
			{
				ISymbol? symbol = semanticModel.GetSymbolInfo(initializer, cancellationToken).Symbol;
				return symbol is IFieldSymbol field &&
					field.Name == "Empty" &&
					field.ContainingType.SpecialType == SpecialType.System_String;
			}

			if (IsCollection(property.Type))
			{
				return initializer is CollectionExpressionSyntax collection && collection.Elements.Count == 0;
			}

			return property.Type.IsReferenceType && IsNullForgiving(initializer);
		}

		private static bool IsCollection(ITypeSymbol type)
		{
			if (type is IArrayTypeSymbol)
			{
				return true;
			}

			if (type is not INamedTypeSymbol namedType)
			{
				return false;
			}

			return namedType.AllInterfaces.Any(candidate =>
				candidate.Name == "IEnumerable" &&
				(candidate.ContainingNamespace.ToDisplayString() == "System.Collections" ||
				candidate.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"));
		}

		private static bool IsNullForgiving(ExpressionSyntax initializer)
		{
			return initializer is PostfixUnaryExpressionSyntax suppression &&
				suppression.IsKind(SyntaxKind.SuppressNullableWarningExpression) &&
				suppression.Operand.IsKind(SyntaxKind.NullLiteralExpression);
		}
	}
}
