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
		public async Task FluentModelConfiguration_HasKeyCall_ReportsDiagnostic()
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
						modelBuilder.Entity<Customer>()
							.HasKey(customer => customer.Id);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0005");
		}

		[Test]
		public async Task FluentModelConfiguration_AnnotationEquivalentCall_ReportsOneDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;

				public class Customer
				{
					public string Name { get; set; } = null!;
				}

				public class AppDbContext : DbContext
				{
					protected override void OnModelCreating(ModelBuilder modelBuilder)
					{
						base.OnModelCreating(modelBuilder);
						modelBuilder.Entity<Customer>()
							.Property(customer => customer.Name)
							.HasColumnName("customer_name");
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			Assert.That(diagnostics.Length, Is.EqualTo(1));
			AssertDiagnostic(diagnostics, "EEX0005");
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("HasColumnName"));
		}

		[Test]
		public async Task FluentModelConfiguration_ValueConverter_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

				public class Customer
				{
					public string Secret { get; set; } = null!;
				}

				public static class CustomerModelConfiguration
				{
					public static void Configure(ModelBuilder modelBuilder)
					{
						ValueConverter<string, string> converter = new(
							value => value,
							value => value);
						ConfigureProperty<Customer>(modelBuilder, customer => customer.Secret, converter);
					}

					private static void ConfigureProperty<TEntity>(
						ModelBuilder modelBuilder,
						System.Linq.Expressions.Expression<System.Func<TEntity, string>> property,
						ValueConverter<string, string> converter)
						where TEntity : class
					{
						modelBuilder.Entity<TEntity>()
							.Property(property)
							.HasConversion(converter);
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task FluentModelConfiguration_ShadowProperties_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;

				public class Customer
				{
					public int Id { get; set; }
				}

				public static class IntegrityModelConfiguration
				{
					public static void Configure(ModelBuilder modelBuilder)
					{
						modelBuilder.Entity<Customer>()
							.Property<byte[]?>("IntegrityMac")
							.HasColumnName("integrity_mac")
							.IsConcurrencyToken();
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task FluentModelConfiguration_NonGenericShadowProperties_DoesNotReportDiagnostic()
		{
			const string source = """
				using Microsoft.EntityFrameworkCore;
				using Microsoft.EntityFrameworkCore.Metadata.Builders;
				using System;

				public class Customer
				{
					public int Id { get; set; }
				}

				public static class IntegrityModelConfiguration
				{
					private const string VersionProperty = "IntegrityVersion";
					private const string VersionColumn = "integrity_version";
					private const string MacProperty = "IntegrityMac";
					private const string MacColumn = "integrity_mac";

					public static void Configure(ModelBuilder modelBuilder)
					{
						Type[] protectedTypes = [typeof(Customer)];
						foreach (Type entityType in protectedTypes)
						{
							EntityTypeBuilder entity = modelBuilder.Entity(entityType);
							entity.Property<int?>(VersionProperty)
								.HasColumnName(VersionColumn);
							entity.Property<byte[]?>(MacProperty)
								.HasColumnName(MacColumn)
								.IsConcurrencyToken();
						}
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EfFluentModelConfigurationAnalyzer());

			Assert.That(diagnostics, Is.Empty);
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
