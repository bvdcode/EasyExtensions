using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class EntityPropertyInitializerAnalyzerTests
	{
		[Test]
		public async Task PrimitiveDefaultValue_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public bool IsActive { get; set; } = true;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task NullableReferenceNullForgiving_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public string? Name { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task NonNullableStringNullForgiving_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public string Name { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task NonNullableStringEmpty_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public string Name { get; set; } = string.Empty;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task NonNullableByteArrayNullForgiving_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class ChunkReference : BaseEntity<Guid>
				{
					public byte[] ChunkHash { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task NonNullableByteArrayEmpty_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class ChunkReference : BaseEntity<Guid>
				{
					public byte[] ChunkHash { get; set; } = [];
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			AssertDiagnostic(diagnostics);
		}

		[Test]
		public async Task ApprovedInitializers_DoNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.Collections.Generic;

				public class Customer : BaseEntity<Guid>
				{
					public ICollection<string> Tags { get; set; } = [];
					public object State { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task NotMappedPrimitiveInitializer_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.ComponentModel.DataAnnotations.Schema;

				public class Customer : BaseEntity<Guid>
				{
					[NotMapped]
					public bool IsReady { get; set; } = true;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityPropertyInitializerAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0012"));
		}
	}
}
