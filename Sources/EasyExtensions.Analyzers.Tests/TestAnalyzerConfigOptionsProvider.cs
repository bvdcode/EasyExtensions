using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers.Tests
{
	internal class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
	{
		private readonly AnalyzerConfigOptions options;

		public TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string>? options)
		{
			this.options = new TestAnalyzerConfigOptions(options);
		}

		public override AnalyzerConfigOptions GlobalOptions => options;

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			return options;
		}

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return options;
		}
	}
}
