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

				if (location is null ||
					SymbolHelpers.IsEntity(entity) ||
					HasExplicitNaturalKey(entity))
				{
					continue;
				}

				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.EntityBaseType,
					location,
					entity.Name));
			}
		}

		private static bool HasExplicitNaturalKey(INamedTypeSymbol entity)
		{
			bool hasExplicitKey = false;

			for (INamedTypeSymbol? currentType = entity;
				currentType is not null;
				currentType = currentType.BaseType)
			{
				foreach (ISymbol member in currentType.GetMembers())
				{
					if (member is not IPropertySymbol property || property.IsStatic)
					{
						continue;
					}

					if (property.Name == "Id" || property.Name == entity.Name + "Id")
					{
						return false;
					}

					hasExplicitKey |= SymbolHelpers.GetAttribute(
						property,
						"System.ComponentModel.DataAnnotations",
						"KeyAttribute") is not null;
				}
			}

			return hasExplicitKey;
		}
	}
}
