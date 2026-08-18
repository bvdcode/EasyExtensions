using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TopLevelTypeCountAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.MultipleTopLevelTypes];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSemanticModelAction(AnalyzeSemanticModel);
		}

		private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			SyntaxNode root = context.SemanticModel.SyntaxTree.GetRoot(context.CancellationToken);
			List<BaseTypeDeclarationSyntax> declarations = root.DescendantNodes()
				.OfType<BaseTypeDeclarationSyntax>()
				.Where(IsCountedTopLevelType)
				.ToList();

			if (declarations.Count <= 1 || IsMediatorRequestWithHandlers(context.SemanticModel, declarations, context.CancellationToken))
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.MultipleTopLevelTypes,
				declarations[1].Identifier.GetLocation(),
				declarations.Count));
		}

		private static bool IsCountedTopLevelType(BaseTypeDeclarationSyntax declaration)
		{
			if (declaration.Parent is not CompilationUnitSyntax && declaration.Parent is not BaseNamespaceDeclarationSyntax)
			{
				return false;
			}

			return declaration.IsKind(SyntaxKind.ClassDeclaration) ||
				declaration.IsKind(SyntaxKind.RecordDeclaration) ||
				declaration.IsKind(SyntaxKind.RecordStructDeclaration) ||
				declaration.IsKind(SyntaxKind.InterfaceDeclaration) ||
				declaration.IsKind(SyntaxKind.EnumDeclaration);
		}

		private static bool IsMediatorRequestWithHandlers(
			SemanticModel semanticModel,
			IReadOnlyCollection<BaseTypeDeclarationSyntax> declarations,
			System.Threading.CancellationToken cancellationToken)
		{
			List<INamedTypeSymbol> types = declarations
				.Select(declaration => semanticModel.GetDeclaredSymbol(declaration, cancellationToken))
				.OfType<INamedTypeSymbol>()
				.ToList();
			List<INamedTypeSymbol> requests = types.Where(IsMediatorRequest).ToList();

			if (requests.Count != 1 || types.Count != declarations.Count)
			{
				return false;
			}

			INamedTypeSymbol request = requests[0];
			return types.All(type => SymbolEqualityComparer.Default.Equals(type, request) || IsHandlerFor(type, request));
		}

		private static bool IsMediatorRequest(INamedTypeSymbol type)
		{
			return SymbolHelpers.Implements(type, "EasyExtensions.Mediator.Contracts", "IRequest", 0) ||
				SymbolHelpers.Implements(type, "EasyExtensions.Mediator.Contracts", "IRequest", 1);
		}

		private static bool IsHandlerFor(INamedTypeSymbol type, INamedTypeSymbol request)
		{
			return type.AllInterfaces.Any(candidate =>
				candidate.Name == "IRequestHandler" &&
				(candidate.Arity == 1 || candidate.Arity == 2) &&
				candidate.ContainingNamespace.ToDisplayString() == "EasyExtensions.Mediator" &&
				SymbolEqualityComparer.Default.Equals(candidate.TypeArguments[0], request));
		}
	}
}
