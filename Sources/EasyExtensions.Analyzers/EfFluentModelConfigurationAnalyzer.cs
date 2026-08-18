using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EfFluentModelConfigurationAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EfFluentModelConfiguration];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
		}

		private static void AnalyzeInvocation(OperationAnalysisContext context)
		{
			IInvocationOperation invocation = (IInvocationOperation)context.Operation;
			IMethodSymbol method = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;

			if (!IsModelConfigurationMethod(method))
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.EfFluentModelConfiguration,
				invocation.Syntax.GetLocation(),
				invocation.TargetMethod.Name));
		}

		private static bool IsModelConfigurationMethod(IMethodSymbol method)
		{
			return IsModelBuilderType(method.ContainingType) ||
				method.Parameters.Any(parameter => IsModelBuilderType(parameter.Type));
		}

		private static bool IsModelBuilderType(ITypeSymbol? type)
		{
			if (type is not INamedTypeSymbol namedType)
			{
				return false;
			}

			string namespaceName = namedType.ContainingNamespace.ToDisplayString();
			return namespaceName == "Microsoft.EntityFrameworkCore" && namedType.Name == "ModelBuilder" ||
				namespaceName.StartsWith("Microsoft.EntityFrameworkCore.Metadata.Builders", StringComparison.Ordinal);
		}
	}
}
