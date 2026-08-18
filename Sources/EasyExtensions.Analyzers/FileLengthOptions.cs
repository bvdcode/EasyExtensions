using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EasyExtensions.Analyzers
{
	internal static class FileLengthOptions
	{
		public const int DefaultMaxLines = 400;
		public const string MaxLinesOptionName = "dotnet_code_quality.EEX0001.max_lines";

		public static int GetMaxLines(AnalyzerConfigOptions options)
		{
			if (!options.TryGetValue(MaxLinesOptionName, out string? configuredValue))
			{
				return DefaultMaxLines;
			}

			if (!int.TryParse(configuredValue, NumberStyles.None, CultureInfo.InvariantCulture, out int maxLines))
			{
				return DefaultMaxLines;
			}

			if (maxLines <= 0)
			{
				return DefaultMaxLines;
			}

			return maxLines;
		}
	}
}
