using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class EfUntrackedMutationAnalyzerTests
	{
		[TestCase("query.ExecuteDelete()")]
		[TestCase("query.ExecuteDeleteAsync()")]
		[TestCase("query.ExecuteDeleteAsync(CancellationToken.None)")]
		[TestCase("query.ExecuteUpdate(setters => setters.SetProperty(row => row.Score, 1))")]
		[TestCase("query.ExecuteUpdateAsync(setters => setters.SetProperty(row => row.Score, 1))")]
		[TestCase("query.ExecuteUpdateAsync(setters => { setters.SetProperty(row => row.Score, 1); }, CancellationToken.None)")]
		[TestCase("EntityFrameworkQueryableExtensions.ExecuteDelete(query)")]
		[TestCase("EntityFrameworkQueryableExtensions.ExecuteDeleteAsync(query)")]
		[TestCase("EntityFrameworkQueryableExtensions.ExecuteUpdate(query, setters => setters.SetProperty(row => row.Score, 1))")]
		[TestCase("EntityFrameworkQueryableExtensions.ExecuteUpdateAsync(query, setters => setters.SetProperty(row => row.Score, 1))")]
		[TestCase("EfQueries.ExecuteDelete(query)")]
		[TestCase("query.Where(row => row.Score > 0).ExecuteDeleteAsync()")]
		[TestCase("query.AsNoTracking().ExecuteUpdateAsync(setters => setters.SetProperty(row => row.Score, 1))")]
		[TestCase("query?.ExecuteDelete()")]
		public async Task BulkMutation_ReportsOneError(string expression)
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync($"_ = {expression};");

			AssertDiagnostic(diagnostics);
		}

		[TestCase("Func<int> mutate = query.ExecuteDelete;")]
		[TestCase("Func<CancellationToken, Task<int>> mutate = query.ExecuteDeleteAsync;")]
		[TestCase("Func<Action<UpdateSettersBuilder<Customer>>, int> mutate = query.ExecuteUpdate;")]
		[TestCase("Func<Action<UpdateSettersBuilder<Customer>>, CancellationToken, Task<int>> mutate = query.ExecuteUpdateAsync;")]
		[TestCase("Func<IQueryable<Customer>, int> mutate = EntityFrameworkQueryableExtensions.ExecuteDelete<Customer>;")]
		[TestCase("Func<IQueryable<Customer>, CancellationToken, Task<int>> mutate = EfQueries.ExecuteDeleteAsync<Customer>;")]
		[TestCase("Func<int> mutate = () => query.ExecuteDelete(); _ = mutate();")]
		public async Task MethodReference_ReportsOneError(string statement)
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(statement);

			AssertDiagnostic(diagnostics);
		}

		[TestCase("context.Add(new Customer());")]
		[TestCase("context.Update(new Customer()); context.SaveChanges();")]
		[TestCase("context.Remove(new Customer()); _ = context.SaveChangesAsync();")]
		[TestCase("context.AddRange(new Customer()); context.UpdateRange(new Customer()); context.RemoveRange(new Customer());")]
		[TestCase("context.Set<Customer>().Update(new Customer()); context.Set<Customer>().Remove(new Customer());")]
		[TestCase("_ = query.AsNoTracking().Where(row => row.Score > 0).ToListAsync();")]
		[TestCase("_ = query.AsNoTrackingWithIdentityResolution().ToListAsync();")]
		[TestCase("_ = nameof(EntityFrameworkQueryableExtensions.ExecuteDelete);")]
		public async Task TrackedOperationsAndReads_DoNotReport(string statement)
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(statement);

			Assert.That(diagnostics, Is.Empty);
		}

		[TestCase("ExecuteUpdate")]
		[TestCase("ExecuteUpdateAsync")]
		[TestCase("ExecuteDelete")]
		[TestCase("ExecuteDeleteAsync")]
		public async Task UnrelatedMethodWithSameName_DoesNotReport(string methodName)
		{
			string source = $$"""
				namespace Microsoft.EntityFrameworkCore
				{
					public static class CustomOperations
					{
						public static int {{methodName}}() => 0;
					}
				}
				namespace Application
				{
					public static class RelationalQueryableExtensions
					{
						public static int {{methodName}}() => 0;
					}
					public class Repository
					{
						public void Run()
						{
							_ = Microsoft.EntityFrameworkCore.CustomOperations.{{methodName}}();
							_ = RelationalQueryableExtensions.{{methodName}}();
							System.Func<int> operation = RelationalQueryableExtensions.{{methodName}};
						}
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source, analyzer: new EfUntrackedMutationAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[TestCase("ExecuteUpdate")]
		[TestCase("ExecuteUpdateAsync")]
		[TestCase("ExecuteDelete")]
		[TestCase("ExecuteDeleteAsync")]
		public async Task RelationalDeclaringType_ReportsDiagnostic(string methodName)
		{
			string source = $$"""
				namespace Microsoft.EntityFrameworkCore
				{
					public static class RelationalQueryableExtensions
					{
						public static int {{methodName}}<T>(this System.Linq.IQueryable<T> query) => 0;
					}
				}
				public class Repository
				{
					public void Run(System.Linq.IQueryable<int> query)
					{
						_ = Microsoft.EntityFrameworkCore.RelationalQueryableExtensions.{{methodName}}(query);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source, analyzer: new EfUntrackedMutationAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task SeparateMutations_ReportSeparateDiagnostics()
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(
				"query.ExecuteDelete(); query.ExecuteUpdate(setters => setters.SetProperty(row => row.Score, 1));");

			Assert.That(diagnostics, Has.Length.EqualTo(2));
			Assert.That(diagnostics[0].Location.SourceSpan, Is.Not.EqualTo(diagnostics[1].Location.SourceSpan));
		}

		[Test]
		public async Task GeneratedCode_DoesNotReport()
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("query.ExecuteDelete();", "Generated.g.cs");

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task LocalSuppression_DoesNotReport()
		{
			ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
				#pragma warning disable EEX0015
				query.ExecuteDelete();
				#pragma warning restore EEX0015
				""");

			Assert.That(diagnostics, Is.Empty);
		}

		private static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string statement, string filePath = "Test.cs")
		{
			string source = $$"""
				using System;
				using System.Linq;
				using System.Threading;
				using System.Threading.Tasks;
				using Microsoft.EntityFrameworkCore;
				using Microsoft.EntityFrameworkCore.Query;
				using EfQueries = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;

				public class Customer
				{
					public int Score { get; set; }
				}
				public class Repository
				{
					public void Run(DbContext context, IQueryable<Customer> query)
					{
						{{statement}}
					}
				}
				""";

			return AnalyzerTestRunner.GetDiagnosticsAsync(source, filePath, analyzer: new EfUntrackedMutationAnalyzer());
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics)
		{
			Assert.That(diagnostics, Has.Length.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0015"));
			Assert.That(diagnostics[0].Severity, Is.EqualTo(DiagnosticSeverity.Error));
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("SaveChanges"));
		}
	}
}
