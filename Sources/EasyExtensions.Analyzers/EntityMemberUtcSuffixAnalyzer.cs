using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityMemberUtcSuffixAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EntityMemberUtcSuffix];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSymbolAction(AnalyzeMember, SymbolKind.Property, SymbolKind.Field);
		}

		private static void AnalyzeMember(SymbolAnalysisContext context)
		{
			ISymbol member = context.Symbol;

			if (member.IsStatic ||
				member.ContainingType is null ||
				EntityModel.IsNotMapped(member) ||
				!SymbolHelpers.IsEntity(member.ContainingType) ||
				!member.Name.EndsWith("Utc", StringComparison.Ordinal))
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.EntityMemberUtcSuffix,
				member.Locations[0],
				member.Name));
		}
	}
}
