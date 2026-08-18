using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers.Tests
{
	internal class TestAnalyzerConfigOptions : AnalyzerConfigOptions
	{
		private readonly ImmutableDictionary<string, string> options;

		public TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string>? options)
		{
			if (options is null)
			{
				this.options = ImmutableDictionary<string, string>.Empty;
				return;
			}

			this.options = options.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
		}

		public override bool TryGetValue(string key, out string value)
		{
			if (options.TryGetValue(key, out string? configuredValue))
			{
				value = configuredValue;
				return true;
			}

			value = string.Empty;
			return false;
		}
	}
}
