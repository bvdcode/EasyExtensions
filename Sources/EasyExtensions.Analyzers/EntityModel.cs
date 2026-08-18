using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace EasyExtensions.Analyzers
{
	internal static class EntityModel
	{
		public static HashSet<INamedTypeSymbol> DiscoverEntityTypes(Compilation compilation)
		{
			List<INamedTypeSymbol> allTypes = SymbolHelpers
				.GetAllNamedTypes(compilation.Assembly.GlobalNamespace)
				.ToList();
			HashSet<INamedTypeSymbol> entities = new(SymbolEqualityComparer.Default);
			Queue<INamedTypeSymbol> pendingEntities = new();

			foreach (INamedTypeSymbol type in allTypes)
			{
				if (HasTableAttribute(type) || SymbolHelpers.IsEntity(type))
				{
					AddEntity(type, entities, pendingEntities);
				}

				foreach (IPropertySymbol property in type.GetMembers().OfType<IPropertySymbol>())
				{
					if (TryGetDbSetEntityType(property.Type, out INamedTypeSymbol? entityType))
					{
						AddEntity(entityType, entities, pendingEntities);
					}
				}
			}

			while (pendingEntities.Count > 0)
			{
				INamedTypeSymbol entity = pendingEntities.Dequeue();
				AddSourceBaseTypes(entity, compilation, entities, pendingEntities);

				foreach (IPropertySymbol property in entity.GetMembers().OfType<IPropertySymbol>())
				{
					if (property.IsStatic || property.IsIndexer || IsNotMapped(property))
					{
						continue;
					}

					if (TryGetNavigationCandidate(property.Type, compilation, out INamedTypeSymbol? targetType, out _))
					{
						AddEntity(targetType, entities, pendingEntities);
					}
				}
			}

			return entities;
		}

		public static bool TryGetNavigationTarget(
			ITypeSymbol propertyType,
			ISet<INamedTypeSymbol> entities,
			out INamedTypeSymbol? targetType,
			out bool isCollection)
		{
			if (TryGetCollectionElementType(propertyType, out INamedTypeSymbol? elementType) &&
				elementType is not null &&
				entities.Contains(elementType))
			{
				targetType = elementType;
				isCollection = true;
				return true;
			}

			if (propertyType is INamedTypeSymbol namedType && entities.Contains(namedType))
			{
				targetType = namedType;
				isCollection = false;
				return true;
			}

			targetType = null;
			isCollection = false;
			return false;
		}

		public static bool IsNotMapped(ISymbol symbol)
		{
			return SymbolHelpers.GetAttribute(
				symbol,
				"System.ComponentModel.DataAnnotations.Schema",
				"NotMappedAttribute") is not null;
		}

		private static bool TryGetNavigationCandidate(
			ITypeSymbol propertyType,
			Compilation compilation,
			out INamedTypeSymbol? targetType,
			out bool isCollection)
		{
			if (TryGetCollectionElementType(propertyType, out INamedTypeSymbol? elementType) &&
				elementType is not null &&
				IsEntityCandidate(elementType, compilation))
			{
				targetType = elementType;
				isCollection = true;
				return true;
			}

			if (propertyType is INamedTypeSymbol namedType && IsEntityCandidate(namedType, compilation))
			{
				targetType = namedType;
				isCollection = false;
				return true;
			}

			targetType = null;
			isCollection = false;
			return false;
		}

		private static bool TryGetCollectionElementType(
			ITypeSymbol type,
			out INamedTypeSymbol? elementType)
		{
			if (type is IArrayTypeSymbol arrayType && arrayType.ElementType is INamedTypeSymbol arrayElementType)
			{
				elementType = arrayElementType;
				return true;
			}

			if (type is not INamedTypeSymbol namedType || type.SpecialType == SpecialType.System_String)
			{
				elementType = null;
				return false;
			}

			IEnumerable<INamedTypeSymbol> candidates = new[] { namedType }.Concat(namedType.AllInterfaces);
			INamedTypeSymbol? enumerableType = candidates.FirstOrDefault(candidate =>
				SymbolHelpers.Matches(
					candidate.OriginalDefinition,
					"System.Collections.Generic",
					"IEnumerable",
					1));
			elementType = enumerableType?.TypeArguments[0] as INamedTypeSymbol;
			return elementType is not null;
		}

		private static bool IsEntityCandidate(INamedTypeSymbol type, Compilation compilation)
		{
			return type.TypeKind == TypeKind.Class &&
				!type.IsAnonymousType &&
				SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly) &&
				SymbolHelpers.GetSourceLocation(type) is not null &&
				!IsNotMapped(type) &&
				!HasComplexTypeAttribute(type) &&
				!IsSystemNamespace(type.ContainingNamespace.ToDisplayString());
		}

		private static bool TryGetDbSetEntityType(ITypeSymbol type, out INamedTypeSymbol? entityType)
		{
			if (type is INamedTypeSymbol namedType &&
				SymbolHelpers.Matches(
					namedType.OriginalDefinition,
					"Microsoft.EntityFrameworkCore",
					"DbSet",
					1))
			{
				entityType = namedType.TypeArguments[0] as INamedTypeSymbol;
				return entityType is not null;
			}

			entityType = null;
			return false;
		}

		private static bool HasTableAttribute(INamedTypeSymbol type)
		{
			return SymbolHelpers.GetAttribute(
				type,
				"System.ComponentModel.DataAnnotations.Schema",
				"TableAttribute") is not null;
		}

		private static bool HasComplexTypeAttribute(INamedTypeSymbol type)
		{
			return SymbolHelpers.GetAttribute(
				type,
				"System.ComponentModel.DataAnnotations.Schema",
				"ComplexTypeAttribute") is not null ||
				SymbolHelpers.GetAttribute(type, "Microsoft.EntityFrameworkCore", "ComplexTypeAttribute") is not null;
		}

		private static bool IsSystemNamespace(string namespaceName)
		{
			return namespaceName == "System" || namespaceName.StartsWith("System.", StringComparison.Ordinal);
		}

		private static void AddSourceBaseTypes(
			INamedTypeSymbol entity,
			Compilation compilation,
			HashSet<INamedTypeSymbol> entities,
			Queue<INamedTypeSymbol> pendingEntities)
		{
			for (INamedTypeSymbol? baseType = entity.BaseType;
				baseType is not null && IsEntityCandidate(baseType, compilation);
				baseType = baseType.BaseType)
			{
				AddEntity(baseType, entities, pendingEntities);
			}
		}

		private static void AddEntity(
			INamedTypeSymbol? entity,
			HashSet<INamedTypeSymbol> entities,
			Queue<INamedTypeSymbol> pendingEntities)
		{
			if (entity is not null && entities.Add(entity))
			{
				pendingEntities.Enqueue(entity);
			}
		}
	}
}
