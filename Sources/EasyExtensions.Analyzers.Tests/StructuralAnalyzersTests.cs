using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class StructuralAnalyzersTests
	{
		[Test]
		public async Task SealedKeyword_SealedClass_ReportsDiagnostic()
		{
			const string source = "public sealed class Example { }";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new SealedKeywordAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0002");
		}

		[Test]
		public async Task SealedKeyword_UnsealedClass_DoesNotReportDiagnostic()
		{
			const string source = "public class Example { }";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new SealedKeywordAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task TopLevelTypes_TwoClasses_ReportsDiagnostic()
		{
			const string source = "public class First { } public class Second { }";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new TopLevelTypeCountAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0003");
		}

		[Test]
		public async Task TopLevelTypes_MediatorRequestAndHandler_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.Mediator;
				using EasyExtensions.Mediator.Contracts;
				using System.Threading;
				using System.Threading.Tasks;

				namespace Tests
				{
					public record Ping : IRequest<string>;

					public class PingHandler : IRequestHandler<Ping, string>
					{
						public Task<string> Handle(Ping request, CancellationToken cancellationToken)
						{
							return Task.FromResult("pong");
						}
					}
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new TopLevelTypeCountAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EntityDto_OwnIdWithoutBaseDto_ReportsDiagnostic()
		{
			const string source = """
				using System;

				public class CustomerDto
				{
					public Guid Id { get; set; }
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityDtoBaseTypeAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0007");
		}

		[Test]
		public async Task EntityDto_DerivesFromBaseDto_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.Models.Dto;
				using System;

				public class CustomerDto : BaseDto<Guid>
				{
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EntityDtoBaseTypeAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task QuartzJob_WithoutJobTrigger_ReportsDiagnostic()
		{
			const string source = """
				using Quartz;
				using System.Threading.Tasks;

				public class CleanupJob : IJob
				{
					public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new QuartzJobTriggerAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0008");
		}

		[Test]
		public async Task QuartzJob_WithJobTrigger_DoesNotReportDiagnostic()
		{
			const string source = """
				using EasyExtensions.Quartz.Attributes;
				using Quartz;
				using System.Threading.Tasks;

				[JobTrigger(seconds: 10)]
				public class CleanupJob : IJob
				{
					public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
				}
				""";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new QuartzJobTriggerAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task EnumDto_MatchesEnumName_ReportsDiagnostic()
		{
			const string source = "public enum Status { Active } public class StatusDto { }";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EnumDtoAnalyzer());

			AssertDiagnostic(diagnostics, "EEX0013");
		}

		[Test]
		public async Task EnumDto_DifferentName_DoesNotReportDiagnostic()
		{
			const string source = "public enum Status { Active } public class StatusResponseDto { }";

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				analyzer: new EnumDtoAnalyzer());

			Assert.That(diagnostics, Is.Empty);
		}

		private static void AssertDiagnostic(ImmutableArray<Diagnostic> diagnostics, string diagnosticId)
		{
			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo(diagnosticId));
		}
	}
}
