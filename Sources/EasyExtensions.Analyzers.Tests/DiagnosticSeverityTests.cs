using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class DiagnosticSeverityTests
	{
		[Test]
		public void AllDiagnostics_DefaultToErrors()
		{
			DiagnosticAnalyzer[] analyzers =
			[
				new FileLengthAnalyzer(),
				new SealedKeywordAnalyzer(),
				new TopLevelTypeCountAnalyzer(),
				new DeleteBehaviorRestrictAnalyzer(),
				new EfFluentModelConfigurationAnalyzer(),
				new EntityBaseTypeAnalyzer(),
				new EntityDtoBaseTypeAnalyzer(),
				new QuartzJobTriggerAnalyzer(),
				new RawSqlAnalyzer(),
				new EntityMemberUtcSuffixAnalyzer(),
				new ReflectionUsageAnalyzer(),
				new EntityPropertyInitializerAnalyzer(),
				new EnumDtoAnalyzer(),
				new ExplicitLocalVariableTypeAnalyzer()
			];

			foreach (DiagnosticAnalyzer analyzer in analyzers)
			{
				foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
				{
					Assert.That(
						descriptor.DefaultSeverity,
						Is.EqualTo(DiagnosticSeverity.Error),
						descriptor.Id);
					Assert.That(
						descriptor.CustomTags,
						Does.Contain(WellKnownDiagnosticTags.CustomSeverityConfigurable),
						descriptor.Id);
				}
			}
		}
	}
}
