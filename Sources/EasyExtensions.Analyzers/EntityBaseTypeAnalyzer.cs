using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityBaseTypeAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EntityBaseType];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCompilationAction(AnalyzeCompilation);
		}

		private static void AnalyzeCompilation(CompilationAnalysisContext context)
		{
			HashSet<INamedTypeSymbol> entities = EntityModel.DiscoverEntityTypes(context.Compilation);

			foreach (INamedTypeSymbol entity in entities)
			{
				Location? location = SymbolHelpers.GetSourceLocation(entity);

				if (location is null || SymbolHelpers.IsEntity(entity))
				{
					continue;
				}

				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.EntityBaseType,
					location,
					entity.Name));
			}
		}
	}
}
