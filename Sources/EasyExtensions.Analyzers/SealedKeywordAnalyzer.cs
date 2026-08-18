using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SealedKeywordAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.SealedKeyword];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
		}

		private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);

			foreach (SyntaxToken token in root.DescendantTokens())
			{
				if (token.IsKind(SyntaxKind.SealedKeyword))
				{
					context.ReportDiagnostic(Diagnostic.Create(
						DiagnosticDescriptors.SealedKeyword,
						token.GetLocation()));
				}
			}
		}
	}
}
