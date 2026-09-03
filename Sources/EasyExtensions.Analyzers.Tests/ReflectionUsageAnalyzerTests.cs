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
		public async Task TypeIsAssignableFrom_DoesNotReportDiagnostic()
		{
			const string source = """
				using System;

				public class TypeRelationship
				{
					public bool Matches(Type contract, Type candidate)
					{
						return contract.IsAssignableFrom(candidate);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task RuntimeTypeNames_DoNotReportDiagnostic()
		{
			const string source = """
				public class Worker
				{
					public string Name => GetType().Name;

					public string FullName => GetType().FullName ?? string.Empty;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task MemberInfoName_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.Reflection;

				public class Reader
				{
					public string Read(MemberInfo member) => member.Name;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task RuntimeTypeDiscovery_ReportsSingleDiagnostic()
		{
			const string source = """
				public class Reader
				{
					public void Read(object value)
					{
						value.GetType().GetMethods();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task MetadataNameAfterDiscovery_ReportsSingleDiagnostic()
		{
			const string source = """
				public class Customer
				{
					public string Name { get; set; } = string.Empty;
				}

				public class Reader
				{
					public string Read() => typeof(Customer).GetProperty("Name")!.Name;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task DiscoveryAndInvocationApis_ReportOneDiagnosticEach()
		{
			const string source = """
				using System;
				using System.Reflection;

				public class Customer
				{
				}

				public class Reader
				{
					public void Read(MethodInfo method)
					{
						typeof(Customer).GetMethods();
						typeof(Customer).GetProperties();
						typeof(Customer).GetCustomAttributes();
						method.Invoke(null, null);
						Activator.CreateInstance(typeof(Customer));
						Assembly.Load("Example");
						Type.GetType("Example.Customer");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new ReflectionUsageAnalyzer());

			AssertDiagnostics(diagnostics, 7);
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
			AssertDiagnostics(diagnostics, 1);
		}

		private static void AssertDiagnostics(ImmutableArray<Diagnostic> diagnostics, int expectedCount)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(expectedCount));
			Assert.That(diagnostics, Has.All.Matches<Diagnostic>(diagnostic => diagnostic.Id == "EEX0011"));
		}
	}
}
