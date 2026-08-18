using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class QuartzJobTriggerAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.QuartzJobTrigger];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
		}

		private static void AnalyzeType(SymbolAnalysisContext context)
		{
			INamedTypeSymbol type = (INamedTypeSymbol)context.Symbol;

			if (type.TypeKind != TypeKind.Class ||
				type.IsAbstract ||
				!SymbolHelpers.Implements(type, "Quartz", "IJob", 0) ||
				SymbolHelpers.GetAttribute(
					type,
					"EasyExtensions.Quartz.Attributes",
					"JobTriggerAttribute") is not null)
			{
				return;
			}

			Location? location = SymbolHelpers.GetSourceLocation(type);

			if (location is not null)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.QuartzJobTrigger,
					location,
					type.Name));
			}
		}
	}
}
