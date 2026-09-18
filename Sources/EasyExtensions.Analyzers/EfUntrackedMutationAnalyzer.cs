using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EfUntrackedMutationAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.EfUntrackedMutation];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
			context.RegisterOperationAction(AnalyzeMethodReference, OperationKind.MethodReference);
		}

		private static void AnalyzeInvocation(OperationAnalysisContext context)
		{
			IInvocationOperation invocation = (IInvocationOperation)context.Operation;
			AnalyzeMethod(context, invocation.TargetMethod);
		}

		private static void AnalyzeMethodReference(OperationAnalysisContext context)
		{
			IMethodReferenceOperation reference = (IMethodReferenceOperation)context.Operation;
			AnalyzeMethod(context, reference.Method);
		}

		private static void AnalyzeMethod(OperationAnalysisContext context, IMethodSymbol method)
		{
			method = method.ReducedFrom ?? method;

			if (method.Name is not ("ExecuteUpdate" or "ExecuteUpdateAsync" or "ExecuteDelete" or "ExecuteDeleteAsync") ||
				!(SymbolHelpers.Matches(method.ContainingType, "Microsoft.EntityFrameworkCore", "EntityFrameworkQueryableExtensions", 0) ||
					SymbolHelpers.Matches(method.ContainingType, "Microsoft.EntityFrameworkCore", "RelationalQueryableExtensions", 0)))
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.EfUntrackedMutation,
				context.Operation.Syntax.GetLocation(),
				method.Name));
		}
	}
}
