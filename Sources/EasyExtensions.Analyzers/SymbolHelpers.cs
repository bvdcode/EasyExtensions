using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EasyExtensions.Analyzers
{
	internal static class SymbolHelpers
	{
		public const string BaseDtoNamespace = "EasyExtensions.Models.Dto";
		public const string BaseEntityNamespace = "EasyExtensions.EntityFrameworkCore.Abstractions";

		public static bool IsEntity(INamedTypeSymbol? type)
		{
			return IsOrInheritsFrom(type, BaseEntityNamespace, "BaseEntity", 1);
		}

		public static bool IsOrInheritsFrom(
			INamedTypeSymbol? type,
			string namespaceName,
			string typeName,
			int arity)
		{
			for (INamedTypeSymbol? currentType = type; currentType is not null; currentType = currentType.BaseType)
			{
				if (Matches(currentType.OriginalDefinition, namespaceName, typeName, arity))
				{
					return true;
				}
			}

			return false;
		}

		public static bool Implements(
			INamedTypeSymbol type,
			string namespaceName,
			string interfaceName,
			int arity)
		{
			return type.AllInterfaces.Any(candidate =>
				Matches(candidate.OriginalDefinition, namespaceName, interfaceName, arity));
		}

		public static bool Matches(
			INamedTypeSymbol type,
			string namespaceName,
			string typeName,
			int arity)
		{
			return type.Name == typeName &&
				type.Arity == arity &&
				type.ContainingNamespace.ToDisplayString() == namespaceName;
		}

		public static AttributeData? GetAttribute(
			ISymbol symbol,
			string namespaceName,
			string attributeName)
		{
			return symbol.GetAttributes().FirstOrDefault(attribute =>
				attribute.AttributeClass is not null &&
				Matches(attribute.AttributeClass, namespaceName, attributeName, 0));
		}

		public static Location? GetSourceLocation(INamedTypeSymbol type)
		{
			return type.Locations.FirstOrDefault(location => location.IsInSource);
		}

		public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(INamespaceSymbol namespaceSymbol)
		{
			foreach (INamespaceOrTypeSymbol member in namespaceSymbol.GetMembers())
			{
				if (member is INamespaceSymbol childNamespace)
				{
					foreach (INamedTypeSymbol childType in GetAllNamedTypes(childNamespace))
					{
						yield return childType;
					}
				}
				else if (member is INamedTypeSymbol namedType)
				{
					foreach (INamedTypeSymbol type in GetTypeAndNestedTypes(namedType))
					{
						yield return type;
					}
				}
			}
		}

		private static IEnumerable<INamedTypeSymbol> GetTypeAndNestedTypes(INamedTypeSymbol type)
		{
			yield return type;

			foreach (INamedTypeSymbol nestedType in type.GetTypeMembers())
			{
				foreach (INamedTypeSymbol childType in GetTypeAndNestedTypes(nestedType))
				{
					yield return childType;
				}
			}
		}
	}
}
