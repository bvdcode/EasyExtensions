using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ReflectionUsageAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.Reflection];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
			context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
			context.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
		}

		private static void AnalyzeInvocation(OperationAnalysisContext context)
		{
			IInvocationOperation invocation = (IInvocationOperation)context.Operation;
			IMethodSymbol method = invocation.TargetMethod;

			if (IsReflectionType(method.ContainingType) ||
				IsDynamicInvoke(method) ||
				IsAttributeInspection(method))
			{
				Report(context, invocation.Syntax.GetLocation(), method.ToDisplayString());
			}
		}

		private static void AnalyzeObjectCreation(OperationAnalysisContext context)
		{
			IObjectCreationOperation creation = (IObjectCreationOperation)context.Operation;

			if (creation.Type is not null && IsReflectionType(creation.Type))
			{
				Report(context, creation.Syntax.GetLocation(), creation.Type.ToDisplayString());
			}
		}

		private static void AnalyzePropertyReference(OperationAnalysisContext context)
		{
			IPropertyReferenceOperation reference = (IPropertyReferenceOperation)context.Operation;

			if (IsReflectionType(reference.Property.ContainingType) &&
				!IsSafeMetadataName(reference.Property))
			{
				Report(context, reference.Syntax.GetLocation(), reference.Property.ToDisplayString());
			}
		}

		private static bool IsReflectionType(ITypeSymbol type)
		{
			string namespaceName = type.ContainingNamespace.ToDisplayString();
			return namespaceName.StartsWith("System.Reflection", StringComparison.Ordinal) ||
				namespaceName == "System" && (type.Name == "Type" || type.Name == "Activator");
		}

		private static bool IsSafeMetadataName(IPropertySymbol property)
		{
			if (property.Name == "FullName" &&
				SymbolHelpers.Matches(property.ContainingType, "System", "Type", 0))
			{
				return true;
			}

			return property.Name == "Name" &&
				SymbolHelpers.IsOrInheritsFrom(
					property.ContainingType,
					"System.Reflection",
					"MemberInfo",
					0);
		}

		private static bool IsDynamicInvoke(IMethodSymbol method)
		{
			return method.Name == "DynamicInvoke" &&
				SymbolHelpers.IsOrInheritsFrom(method.ContainingType, "System", "Delegate", 0);
		}

		private static bool IsAttributeInspection(IMethodSymbol method)
		{
			return SymbolHelpers.Matches(method.ContainingType, "System", "Attribute", 0) &&
				(method.Name.StartsWith("GetCustomAttribute", StringComparison.Ordinal) || method.Name == "IsDefined");
		}

		private static void Report(OperationAnalysisContext context, Location location, string apiName)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.Reflection,
				location,
				apiName));
		}
	}
}
