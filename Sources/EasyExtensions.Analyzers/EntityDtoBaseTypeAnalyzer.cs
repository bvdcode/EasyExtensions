using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityDtoBaseTypeAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EntityDtoBaseType];

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
				!type.Name.EndsWith("Dto", StringComparison.Ordinal) ||
				SymbolHelpers.IsOrInheritsFrom(type, SymbolHelpers.BaseDtoNamespace, "BaseDto", 1) ||
				!DeclaresEntityIdProperty(type))
			{
				return;
			}

			Location? location = SymbolHelpers.GetSourceLocation(type);

			if (location is not null)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.EntityDtoBaseType,
					location,
					type.Name));
			}
		}

		private static bool DeclaresEntityIdProperty(INamedTypeSymbol type)
		{
			return type.GetMembers("Id")
				.OfType<IPropertySymbol>()
				.Any(property => !property.IsStatic && IsEntityIdentifier(property.Type));
		}

		private static bool IsEntityIdentifier(ITypeSymbol type)
		{
			if (!type.IsValueType)
			{
				return false;
			}

			return type is not INamedTypeSymbol namedType ||
				namedType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;
		}
	}
}
