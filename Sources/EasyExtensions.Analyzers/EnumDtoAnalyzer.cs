using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EnumDtoAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EnumDto];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCompilationAction(AnalyzeCompilation);
		}

		private static void AnalyzeCompilation(CompilationAnalysisContext context)
		{
			List<INamedTypeSymbol> allTypes = SymbolHelpers
				.GetAllNamedTypes(context.Compilation.Assembly.GlobalNamespace)
				.ToList();
			Dictionary<string, INamedTypeSymbol> enums = allTypes
				.Where(type => type.TypeKind == TypeKind.Enum)
				.GroupBy(type => type.Name, StringComparer.Ordinal)
				.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

			foreach (INamedTypeSymbol dto in allTypes)
			{
				if (dto.TypeKind != TypeKind.Class ||
					dto.IsAbstract ||
					!dto.Name.EndsWith("Dto", StringComparison.Ordinal))
				{
					continue;
				}

				string enumName = dto.Name.Substring(0, dto.Name.Length - "Dto".Length);

				if (!enums.TryGetValue(enumName, out INamedTypeSymbol? enumType))
				{
					continue;
				}

				Location? location = SymbolHelpers.GetSourceLocation(dto);

				if (location is not null)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						DiagnosticDescriptors.EnumDto,
						location,
						dto.Name,
						enumType.Name));
				}
			}
		}
	}
}
