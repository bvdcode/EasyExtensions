using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class RawSqlAnalyzerTests
	{
		[Test]
		public async Task EfFromSqlRaw_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;

				public class Customer
				{
					public int Id { get; set; }
				}

				public class Repository
				{
					public void Query(DbContext context)
					{
						context.Set<Customer>().FromSqlRaw("select * from customers");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task DbCommandTextAssignment_ReportsDiagnostic()
		{
			const string source = """
				using System.Data.Common;

				public class Repository
				{
					public void Query(DbCommand command)
					{
						command.CommandText = "select * from customers";
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task ConstantCreateExtensionAsync_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class DatabaseInitializer
				{
					private const string CreateExtensionSql = "CREATE EXTENSION IF NOT EXISTS pgcrypto;";

					public async Task InitializeAsync(DbContext context)
					{
						await context.Database.ExecuteSqlRawAsync(CreateExtensionSql);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task InterpolatedCreateExtensionAsync_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class DatabaseInitializer
				{
					public async Task InitializeAsync(DbContext context, string extension)
					{
						await context.Database.ExecuteSqlRawAsync($"CREATE EXTENSION IF NOT EXISTS {extension};");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task RuntimeCreateExtensionStringAsync_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class DatabaseInitializer
				{
					public async Task InitializeAsync(DbContext context)
					{
						string sql = "CREATE EXTENSION IF NOT EXISTS pgcrypto;";
						await context.Database.ExecuteSqlRawAsync(sql);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task ConstantDmlAsync_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class DatabaseInitializer
				{
					public async Task InitializeAsync(DbContext context)
					{
						await context.Database.ExecuteSqlRawAsync("DELETE FROM customers;");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task ArbitraryConstantDdlAsync_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Threading.Tasks;

				public class DatabaseInitializer
				{
					public async Task InitializeAsync(DbContext context)
					{
						await context.Database.ExecuteSqlRawAsync("CREATE TABLE example (id integer);");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task ConstantCreateExtensionSync_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;

				public class DatabaseInitializer
				{
					public void Initialize(DbContext context)
					{
						context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pgcrypto;");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task EfLinqQuery_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using System.Linq;

				public class Customer
				{
					public int Id { get; set; }
				}

				public class Repository
				{
					public void Query(DbContext context)
					{
						IQueryable<Customer> query = context.Set<Customer>().Where(customer => customer.Id > 0);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new RawSqlAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0009"));
		}
	}
}
