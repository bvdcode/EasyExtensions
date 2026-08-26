using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace EasyExtensions.Analyzers.Tests
{
	[TestFixture]
	public class FileLengthAnalyzerTests
	{
		private const string MaxLinesOptionName = "dotnet_code_quality.EEX0001.max_lines";

		[Test]
		public async Task Analyze_FileExceedsDefaultLimit_ReportsDiagnostic()
		{
			string source = CreateClassWithProperties(400);

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(source);

			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].Id, Is.EqualTo("EEX0001"));
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("maximum of 400"));
		}

		[Test]
		public async Task Analyze_ConfiguredLimitAboveMaximum_UsesHardMaximum()
		{
			string source = CreateClassWithProperties(400);
			Dictionary<string, string> options = new()
			{
				[MaxLinesOptionName] = "500"
			};

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				options: options);

			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("maximum of 400"));
		}

		[Test]
		public async Task Analyze_FileIsAtConfiguredLimit_DoesNotReportDiagnostic()
		{
			const string source = """
				namespace Tests
				{
					public class Example
					{
						public int Value { get; set; }
					}
				}
				""";
			Dictionary<string, string> options = new()
			{
				[MaxLinesOptionName] = "7"
			};

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(source, options: options);

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task Analyze_FileExceedsConfiguredLimit_ReportsDiagnostic()
		{
			const string source = """
				namespace Tests
				{
					public class Example
					{
						public int Value { get; set; }
					}
				}
				""";
			Dictionary<string, string> options = new()
			{
				[MaxLinesOptionName] = "6"
			};

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(source, options: options);

			Assert.That(diagnostics.Length, Is.EqualTo(1));
			Assert.That(diagnostics[0].GetMessage(), Does.Contain("contains 7 code lines"));
		}

		[Test]
		public async Task Analyze_CommentAndBlankLinesExceedLimit_DoesNotReportDiagnostic()
		{
			StringBuilder source = new();
			source.AppendLine("namespace Tests");
			source.AppendLine("{");
			source.AppendLine("    public class Example");
			source.AppendLine("    {");

			for (int index = 0; index < 500; index++)
			{
				source.AppendLine("        // Documentation line");
				source.AppendLine();
			}

			source.AppendLine("        public int Value { get; set; }");
			source.AppendLine("    }");
			source.AppendLine("}");
			Dictionary<string, string> options = new()
			{
				[MaxLinesOptionName] = "7"
			};

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(source.ToString(), options: options);

			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task Analyze_GeneratedFileExceedsLimit_DoesNotReportDiagnostic()
		{
			const string source = """
				namespace Tests
				{
					public class Example
					{
						public int Value { get; set; }
					}
				}
				""";
			Dictionary<string, string> options = new()
			{
				[MaxLinesOptionName] = "1"
			};

			ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestRunner.GetDiagnosticsAsync(
				source,
				"Generated.g.cs",
				options);

			Assert.That(diagnostics, Is.Empty);
		}

		private static string CreateClassWithProperties(int propertyCount)
		{
			StringBuilder source = new();
			source.AppendLine("namespace Tests");
			source.AppendLine("{");
			source.AppendLine("    public class Example");
			source.AppendLine("    {");

			for (int index = 0; index < propertyCount; index++)
			{
				source.AppendLine($"        public int Value{index} {{ get; set; }}");
			}

			source.AppendLine("    }");
			source.AppendLine("}");

			return source.ToString();
		}
	}
}
