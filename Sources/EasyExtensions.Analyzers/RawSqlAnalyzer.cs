using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RawSqlAnalyzer : DiagnosticAnalyzer
	{
		private static readonly Regex CreateExtensionPattern = new(
			"""\A\s*CREATE\s+EXTENSION\s+IF\s+NOT\s+EXISTS\s+(?:"(?:""|[^"])+"|[A-Za-z_][A-Za-z0-9_$]*)\s*;\s*\z""",
			RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		private static readonly HashSet<string> EfRawSqlMethodNames = new(StringComparer.Ordinal)
		{
			"ExecuteSql",
			"ExecuteSqlAsync",
			"ExecuteSqlInterpolated",
			"ExecuteSqlInterpolatedAsync",
			"ExecuteSqlRaw",
			"ExecuteSqlRawAsync",
			"FromSql",
			"FromSqlInterpolated",
			"FromSqlRaw",
			"SqlQuery",
			"SqlQueryRaw"
		};

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.RawSql];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
			context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
		}

		private static void AnalyzeInvocation(OperationAnalysisContext context)
		{
			IInvocationOperation invocation = (IInvocationOperation)context.Operation;
			IMethodSymbol method = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;
			string namespaceName = method.ContainingNamespace.ToDisplayString();

			if (!IsEfRawSqlMethod(method, namespaceName) && !IsDapperMethod(method, namespaceName))
			{
				return;
			}

			if (IsConstantCreateExtensionInvocation(invocation, method, namespaceName))
			{
				return;
			}

			Report(context, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name);
		}

		private static void AnalyzeAssignment(OperationAnalysisContext context)
		{
			ISimpleAssignmentOperation assignment = (ISimpleAssignmentOperation)context.Operation;

			if (assignment.Target is not IPropertyReferenceOperation propertyReference ||
				propertyReference.Property.Name != "CommandText" ||
				!SymbolHelpers.IsOrInheritsFrom(
					propertyReference.Property.ContainingType,
					"System.Data.Common",
					"DbCommand",
					0))
			{
				return;
			}

			Report(context, assignment.Syntax.GetLocation(), "DbCommand.CommandText");
		}

		private static bool IsEfRawSqlMethod(IMethodSymbol method, string namespaceName)
		{
			return namespaceName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) &&
				EfRawSqlMethodNames.Contains(method.Name);
		}

		private static bool IsDapperMethod(IMethodSymbol method, string namespaceName)
		{
			return namespaceName == "Dapper" &&
				(method.Name.StartsWith("Query", StringComparison.Ordinal) ||
				method.Name.StartsWith("Execute", StringComparison.Ordinal));
		}

		private static bool IsConstantCreateExtensionInvocation(
			IInvocationOperation invocation,
			IMethodSymbol method,
			string namespaceName)
		{
			if (method.Name != "ExecuteSqlRawAsync" ||
				!namespaceName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
			{
				return false;
			}

			foreach (IArgumentOperation argument in invocation.Arguments)
			{
				if (argument.Parameter?.Type.SpecialType != SpecialType.System_String)
				{
					continue;
				}

				IOperation value = UnwrapConversion(argument.Value);

				if (value is IInterpolatedStringOperation ||
					!value.ConstantValue.HasValue ||
					value.ConstantValue.Value is not string sql)
				{
					return false;
				}

				return CreateExtensionPattern.IsMatch(sql);
			}

			return false;
		}

		private static IOperation UnwrapConversion(IOperation operation)
		{
			while (operation is IConversionOperation conversion)
			{
				operation = conversion.Operand;
			}

			return operation;
		}

		private static void Report(OperationAnalysisContext context, Location location, string apiName)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.RawSql,
				location,
				apiName));
		}
	}
}
