using Microsoft.CodeAnalysis;

namespace EasyExtensions.Analyzers
{
	internal static class DiagnosticDescriptors
	{
		public static readonly DiagnosticDescriptor FileTooLong = new(
			DiagnosticIds.FileTooLong,
			"Source file is too long",
			"Source file contains {0} code lines, exceeding the configured maximum of {1}",
			"Maintainability",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Large source files tend to accumulate multiple responsibilities and become harder to navigate, understand, and maintain.");

		public static readonly DiagnosticDescriptor SealedKeyword = new(
			DiagnosticIds.SealedKeyword,
			"Do not use the sealed keyword",
			"Remove the sealed keyword",
			"Design",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Types and members must remain extensible unless the rule is explicitly suppressed.");

		public static readonly DiagnosticDescriptor MultipleTopLevelTypes = new(
			DiagnosticIds.MultipleTopLevelTypes,
			"Keep one top-level type per file",
			"File contains {0} top-level types; keep each type in a separate file",
			"Maintainability",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Classes, records, interfaces, and enums should be stored in separate files. A mediator request and its handlers may share a file.");

		public static readonly DiagnosticDescriptor DeleteBehaviorMustBeRestrict = new(
			DiagnosticIds.DeleteBehaviorMustBeRestrict,
			"Use restrictive delete behavior",
			"Relationship navigation '{0}' must have a dependent navigation configured with [DeleteBehavior(DeleteBehavior.Restrict)]",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Entity Framework Core relationship navigations must prevent cascade deletes.",
			customTags: [WellKnownDiagnosticTags.CompilationEnd]);

		public static readonly DiagnosticDescriptor EfFluentModelConfiguration = new(
			DiagnosticIds.EfFluentModelConfiguration,
			"Do not configure the EF model with Fluent API",
			"Configure the data model with data annotations instead of '{0}'",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Entity Framework Core model configuration must use data annotations rather than ModelBuilder or metadata builder APIs.");

		public static readonly DiagnosticDescriptor EntityBaseType = new(
			DiagnosticIds.EntityBaseType,
			"Entity must derive from BaseEntity<T>",
			"Entity '{0}' must derive from EasyExtensions.EntityFrameworkCore.Abstractions.BaseEntity<T>",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Entity types exposed through DbSet<T> or marked with TableAttribute must use the EasyExtensions entity base type.",
			customTags: [WellKnownDiagnosticTags.CompilationEnd]);

		public static readonly DiagnosticDescriptor EntityDtoBaseType = new(
			DiagnosticIds.EntityDtoBaseType,
			"Entity DTO must derive from BaseDto<T>",
			"Entity DTO '{0}' must derive from EasyExtensions.Models.Dto.BaseDto<T>",
			"Design",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "DTO types that declare their own Id property represent entities and must use the EasyExtensions DTO base type.");

		public static readonly DiagnosticDescriptor QuartzJobTrigger = new(
			DiagnosticIds.QuartzJobTrigger,
			"Quartz job must declare JobTrigger",
			"Quartz job '{0}' must declare EasyExtensions.Quartz.Attributes.JobTriggerAttribute",
			"Usage",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Concrete Quartz jobs must declare their schedule through JobTriggerAttribute.");

		public static readonly DiagnosticDescriptor RawSql = new(
			DiagnosticIds.RawSql,
			"Do not use raw SQL",
			"Raw SQL API '{0}' is prohibited; use Entity Framework Core queries and models",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Database access must use Entity Framework Core modeled queries rather than raw SQL or command text.");

		public static readonly DiagnosticDescriptor EntityMemberUtcSuffix = new(
			DiagnosticIds.EntityMemberUtcSuffix,
			"Do not suffix entity members with Utc",
			"Entity member '{0}' must not end with 'Utc'",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Entity timestamps are assumed to use the application's UTC convention and should not encode it in member names.");

		public static readonly DiagnosticDescriptor Reflection = new(
			DiagnosticIds.Reflection,
			"Reflection discovery or invocation requires explicit approval",
			"Reflection discovery or invocation API '{0}' requires explicit approval and suppression of this diagnostic",
			"Usage",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Reflection discovery, activation, and invocation APIs may only be used when the diagnostic is explicitly suppressed for an approved use.");

		public static readonly DiagnosticDescriptor EntityPropertyInitializer = new(
			DiagnosticIds.EntityPropertyInitializer,
			"Use an approved entity property initializer",
			"Initializer for entity property '{0}' is not allowed",
			"EntityFrameworkCore",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Entity properties may only initialize non-nullable strings with string.Empty, collections with [], and other non-nullable reference types with null!.");

		public static readonly DiagnosticDescriptor EnumDto = new(
			DiagnosticIds.EnumDto,
			"Do not create DTOs for enums",
			"DTO '{0}' duplicates enum '{1}'; use the enum directly",
			"Design",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "An enum should be used directly in API models and database entities instead of being wrapped in a same-named DTO.",
			customTags: [WellKnownDiagnosticTags.CompilationEnd]);

		public static readonly DiagnosticDescriptor ExplicitLocalVariableType = new(
			DiagnosticIds.ExplicitLocalVariableType,
			"Use an explicit local variable type",
			"Replace 'var' with the explicit local variable type",
			"Style",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Use explicit local variable types except for LINQ, anonymous types, tuple deconstruction, and generic object construction with a visible type.");
	}
}
