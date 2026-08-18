using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class ReflectionUsageAnalyzerTests
	{
		[Test]
		public async Task TypeGetProperty_ReportsDiagnostic()
		{
			const string source = """
				public class Customer
				{
					public string Name { get; set; } = string.Empty;
				}

				public class Reader
				{
					public void Read()
					{
						typeof(Customer).GetProperty("Name");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task TypeOfWithoutInspection_DoesNotReportDiagnostic()
		{
			const string source = """
				using System;

				public class Customer
				{
				}

				public class Reader
				{
					public Type Read() => typeof(Customer);
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task ApprovedSuppression_DoesNotReportDiagnostic()
		{
			const string source = """
				public class Customer
				{
				}

				public class Reader
				{
					public void Read()
					{
						#pragma warning disable EEX0011
						typeof(Customer).GetProperties();
						#pragma warning restore EEX0011
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0011"));
		}
	}
}
