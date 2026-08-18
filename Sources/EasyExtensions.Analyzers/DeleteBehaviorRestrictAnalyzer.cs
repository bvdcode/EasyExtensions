using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DeleteBehaviorRestrictAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.DeleteBehaviorMustBeRestrict];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCompilationAction(AnalyzeCompilation);
		}

		private static void AnalyzeCompilation(CompilationAnalysisContext context)
		{
			INamedTypeSymbol? deleteBehaviorType = context.Compilation.GetTypeByMetadataName(
				"Microsoft.EntityFrameworkCore.DeleteBehavior");
			object? restrictValue = deleteBehaviorType?
				.GetMembers("Restrict")
				.OfType<IFieldSymbol>()
				.FirstOrDefault()?
				.ConstantValue;
			HashSet<INamedTypeSymbol> entities = EntityModel.DiscoverEntityTypes(context.Compilation);
			HashSet<IPropertySymbol> analyzedProperties = new(SymbolEqualityComparer.Default);

			foreach (INamedTypeSymbol entity in entities)
			{
				foreach (IPropertySymbol property in GetProperties(entity))
				{
					if (!analyzedProperties.Add(property) ||
						property.IsStatic ||
						property.IsIndexer ||
						EntityModel.IsNotMapped(property) ||
						!EntityModel.TryGetNavigationTarget(
							property.Type,
							entities,
							out INamedTypeSymbol? targetType,
							out bool isCollection))
					{
						continue;
					}

					if (isCollection &&
						targetType is not null &&
						HasDependentReference(targetType, entity, entities))
					{
						continue;
					}

					if (!isCollection && HasRestrictAttribute(property, restrictValue))
					{
						continue;
					}

					context.ReportDiagnostic(Diagnostic.Create(
						DiagnosticDescriptors.DeleteBehaviorMustBeRestrict,
						property.Locations[0],
						property.Name));
				}
			}
		}

		private static bool HasDependentReference(
			INamedTypeSymbol targetType,
			INamedTypeSymbol sourceType,
			ISet<INamedTypeSymbol> entities)
		{
			return GetProperties(targetType).Any(property =>
				!property.IsStatic &&
				!property.IsIndexer &&
				!EntityModel.IsNotMapped(property) &&
				EntityModel.TryGetNavigationTarget(
					property.Type,
					entities,
					out INamedTypeSymbol? inverseTarget,
					out bool isCollection) &&
				!isCollection &&
				SymbolEqualityComparer.Default.Equals(inverseTarget, sourceType));
		}

		private static bool HasRestrictAttribute(IPropertySymbol property, object? restrictValue)
		{
			AttributeData? attribute = SymbolHelpers.GetAttribute(
				property,
				"Microsoft.EntityFrameworkCore",
				"DeleteBehaviorAttribute");
			return attribute is not null &&
				attribute.ConstructorArguments.Length == 1 &&
				restrictValue is not null &&
				Equals(attribute.ConstructorArguments[0].Value, restrictValue);
		}

		private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol type)
		{
			for (INamedTypeSymbol? currentType = type; currentType is not null; currentType = currentType.BaseType)
			{
				foreach (IPropertySymbol property in currentType.GetMembers().OfType<IPropertySymbol>())
				{
					yield return property;
				}
			}
		}
	}
}
