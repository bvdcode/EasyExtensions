using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class EntityModelAnalyzersTests
	{
		[Test]
		public async Task EntityBase_TableEntityWithoutBaseEntity_ReportsDiagnostic()
		{
			const string source = """
				using System.ComponentModel.DataAnnotations.Schema;

				[Table("customers")]
				public class Customer
				{
					public int Id { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0006");
		}

		[Test]
		public async Task EntityBase_TableEntityWithExplicitNaturalKey_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.ComponentModel.DataAnnotations;
				using System.ComponentModel.DataAnnotations.Schema;

				[Table("chunks")]
				public class Chunk
				{
					[Key]
					public byte[] Hash { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EntityBase_TableEntityWithExplicitIdKey_ReportsDiagnostic()
		{
			const string source = """
				using System;
				using System.ComponentModel.DataAnnotations;
				using System.ComponentModel.DataAnnotations.Schema;

				[Table("customers")]
				public class Customer
				{
					[Key]
					public Guid Id { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0006");
		}

		[Test]
		public async Task EntityBase_DbSetEntityWithBaseEntity_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using Microsoft.EntityFrameworkCore;
				using System;

				public class Customer : BaseEntity<Guid>
				{
				}

				public class AppDbContext : DbContext
				{
					public DbSet<Customer> Customers { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EntityBase_NavigationOnlyEntityWithoutBaseEntity_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using Microsoft.EntityFrameworkCore;
				using System;
				using System.Collections.Generic;

				public class Order : BaseEntity<Guid>
				{
					public ICollection<OrderLine> Lines { get; set; } = [];
				}

				public class OrderLine
				{
					public int Id { get; set; }
				}

				public class AppDbContext : DbContext
				{
					public DbSet<Order> Orders { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0006");
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("OrderLine"));
		}

		[Test]
		public async Task EntityBase_NotMappedReference_DoesNotDiscoverEntity()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.ComponentModel.DataAnnotations.Schema;

				public class Order : BaseEntity<Guid>
				{
					[NotMapped]
					public OrderPreview Preview { get; set; } = null!;
				}

				public class OrderPreview
				{
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityBaseTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task DeleteBehavior_ReferenceNavigationWithoutAttribute_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.ComponentModel.DataAnnotations.Schema;

				public class Parent : BaseEntity<Guid>
				{
				}

				[Table("children")]
				public class Child : BaseEntity<Guid>
				{
					public Parent Parent { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new DeleteBehaviorRestrictAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0004");
		}

		[Test]
		public async Task DeleteBehavior_ReferenceNavigationUsesRestrict_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using Microsoft.EntityFrameworkCore;
				using System;
				using System.ComponentModel.DataAnnotations.Schema;

				public class Parent : BaseEntity<Guid>
				{
				}

				[Table("children")]
				public class Child : BaseEntity<Guid>
				{
					[DeleteBehavior(DeleteBehavior.Restrict)]
					public Parent Parent { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new DeleteBehaviorRestrictAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task DeleteBehavior_UnidirectionalCollection_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.Collections.Generic;
				using System.ComponentModel.DataAnnotations.Schema;

				[Table("parents")]
				public class Parent : BaseEntity<Guid>
				{
					public ICollection<Child> Children { get; set; } = [];
				}

				public class Child : BaseEntity<Guid>
				{
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new DeleteBehaviorRestrictAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0004");
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("Children"));
		}

		[Test]
		public async Task DeleteBehavior_CollectionWithRestrictedDependentNavigation_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using Microsoft.EntityFrameworkCore;
				using System;
				using System.Collections.Generic;
				using System.ComponentModel.DataAnnotations.Schema;

				[Table("parents")]
				public class Parent : BaseEntity<Guid>
				{
					public ICollection<Child> Children { get; set; } = [];
				}

				public class Child : BaseEntity<Guid>
				{
					[DeleteBehavior(DeleteBehavior.Restrict)]
					public Parent Parent { get; set; } = null!;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new DeleteBehaviorRestrictAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task FluentModelConfiguration_ModelBuilderCall_ReportsDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;

				public class Customer
				{
					public int Id { get; set; }
				}

				public class AppDbContext : DbContext
				{
					protected override void OnModelCreating(ModelBuilder modelBuilder)
					{
						modelBuilder.Entity<Customer>();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0005");
		}

		[Test]
		public async Task FluentModelConfiguration_DataAnnotation_DoesNotReportDiagnostic()
		{
			const string source = """
				using System.ComponentModel.DataAnnotations.Schema;

				public class Customer
				{
					[Column("name")]
					public string Name { get; set; } = string.Empty;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EntityUtcSuffix_UtcProperty_ReportsDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public DateTime LastSeenUtc { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityMemberUtcSuffixAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0010");
		}

		[Test]
		public async Task EntityUtcSuffix_AtProperty_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;

				public class Customer : BaseEntity<Guid>
				{
					public DateTime LastSeenAt { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityMemberUtcSuffixAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EntityUtcSuffix_NotMappedUtcProperty_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.EntityFrameworkCore.Abstractions;
				using System;
				using System.ComponentModel.DataAnnotations.Schema;

				public class Customer : BaseEntity<Guid>
				{
					[NotMapped]
					public DateTime PreviewUtc { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityMemberUtcSuffixAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo(diagnosticId));
		}
	}
}
