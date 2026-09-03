using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class ExplicitLocalVariableTypeAnalyzerTests
	{
		[Test]
		public async Task PrimitiveVar_ReportsDiagnostic()
		{
			const string source = """
				public class Example
				{
					public void Run()
					{
						var count = 1;
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task MethodResultVar_ReportsDiagnostic()
		{
			const string source = """
				public class Example
				{
					public string GetName() => string.Empty;

					public void Run()
					{
						var name = GetName();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task LinqVar_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.Linq;

				public class Example
				{
					public void Run(int[] values)
					{
						var positiveValues = values.Where(value => value > 0);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task GenericConstructionVar_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.Collections.Generic;

				public class Example
				{
					public void Run()
					{
						var values = new List<string>();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task OutVar_ReportsDiagnostic()
		{
			const string source = """
				public class Example
				{
					public bool Parse(string text)
					{
						return int.TryParse(text, out var value) && value > 0;
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task AnonymousTypeOutVar_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.Linq;

				public class Example
				{
					public string? Find(int[] values)
					{
						var lookup = values.ToDictionary(
							value => value,
							value => new { Name = value.ToString() });

						return lookup.TryGetValue(1, out var item) ? item.Name : null;
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task AnonymousTypeForeachVar_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.Linq;

				public class Example
				{
					public void Run(int[] values)
					{
						var items = values.Select(value => new { Value = value });

						foreach (var item in items)
						{
							_ = item.Value;
						}
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task NamedTypeLinqForeachVar_ReportsDiagnostic()
		{
			const string source = """
				using System.Linq;

				public class Example
				{
					public void Run(int[] values)
					{
						foreach (var item in values.Select(value => value.ToString()))
						{
							_ = item.Length;
						}
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task TupleDeconstructionVar_DoesNotReportDiagnostic()
		{
			const string source = """
				public class Example
				{
					public int Sum((int Left, int Right) pair)
					{
						var (left, right) = pair;
						return left + right;
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task TypedTupleDeconstructionVar_DoesNotReportDiagnostic()
		{
			const string source = """
				public class Example
				{
					public int Sum((int Left, int Right) pair)
					{
						int left;
						int right;
						(var leftValue, var rightValue) = pair;
						left = leftValue;
						right = rightValue;
						return left + right;
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EfAsyncLinqVar_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class Customer
				{
					public int Id { get; set; }
				}

				public class Example
				{
					public async Task Run(DbContext context)
					{
						var customers = await context.Set<Customer>()
							.AsNoTracking()
							.ToListAsync();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ExplicitLocalVariableTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0014"));
		}
	}
}
