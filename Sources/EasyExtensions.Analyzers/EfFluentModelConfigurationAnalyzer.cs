using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EfFluentModelConfigurationAnalyzer : DiagnosticAnalyzer
	{
		private static readonly HashSet<string> AllowedConfigurationMethods =
			new(StringComparer.Ordinal)
			{
				"Entity",
				"HasConversion",
				"Property"
			};

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

			if (!IsModelConfigurationMethod(method) || IsAllowed(invocation))
			{
				return;
			}

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.EfFluentModelConfiguration,
				invocation.Syntax.GetLocation(),
				invocation.TargetMethod.Name));
		}

		private static bool IsAllowed(IInvocationOperation invocation)
		{
			string methodName = invocation.TargetMethod.Name;

			if (AllowedConfigurationMethods.Contains(methodName))
			{
				return true;
			}

			return IsShadowPropertyChain(invocation);
		}

		private static bool IsShadowPropertyChain(IInvocationOperation invocation)
		{
			foreach (InvocationExpressionSyntax candidate in invocation.Syntax
				.DescendantNodesAndSelf()
				.OfType<InvocationExpressionSyntax>())
			{
				if (candidate.Expression is MemberAccessExpressionSyntax memberAccess &&
					memberAccess.Name.Identifier.ValueText == "Property" &&
					candidate.ArgumentList.Arguments.Any(argument =>
						argument.Expression.IsKind(SyntaxKind.StringLiteralExpression)))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsModelConfigurationMethod(IMethodSymbol method)
		{
			if (IsModelBuilderType(method.ContainingType))
			{
				return true;
			}

			return method.IsExtensionMethod &&
				method.ContainingNamespace.ToDisplayString()
					.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) &&
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
