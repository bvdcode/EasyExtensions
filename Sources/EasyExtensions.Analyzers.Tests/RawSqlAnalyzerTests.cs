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
