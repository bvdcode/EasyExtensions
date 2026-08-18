using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyExtensions.EntityFrameworkCore.Abstractions;
using EasyExtensions.Mediator.Contracts;
using EasyExtensions.Models.Dto;
using EasyExtensions.Quartz.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace EasyExtensions.Analyzers.Tests
{
	internal static class AnalyzerTestRunner
	{
		public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
			string source,
			string filePath = "Test.cs",
			IReadOnlyDictionary<string, string>? options = null,
			DiagnosticAnalyzer? analyzer = null)
		{
			CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
			SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions, filePath);
			ImmutableArray<MetadataReference> metadataReferences = CreateMetadataReferences();
			CSharpCompilation compilation = CSharpCompilation.Create(
				"AnalyzerTests",
				[syntaxTree],
				metadataReferences,
				new CSharpCompilationOptions(
					OutputKind.DynamicallyLinkedLibrary,
					nullableContextOptions: NullableContextOptions.Enable));
			ImmutableArray<Diagnostic> compilationErrors = compilation
				.GetDiagnostics()
				.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
				.ToImmutableArray();

			if (!compilationErrors.IsEmpty)
			{
				throw new InvalidOperationException(string.Join(Environment.NewLine, compilationErrors));
			}

			ImmutableArray<DiagnosticAnalyzer> analyzers = [analyzer ?? new FileLengthAnalyzer()];
			AnalyzerConfigOptionsProvider optionsProvider = new TestAnalyzerConfigOptionsProvider(options);
			AnalyzerOptions analyzerOptions = new(ImmutableArray<AdditionalText>.Empty, optionsProvider);

			return await compilation
				.WithAnalyzers(analyzers, analyzerOptions)
				.GetAnalyzerDiagnosticsAsync();
		}

		private static ImmutableArray<MetadataReference> CreateMetadataReferences()
		{
			string trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
			HashSet<string> assemblyPaths = trustedAssemblies
				.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
				.ToHashSet(StringComparer.OrdinalIgnoreCase);
			Type[] productTypes =
			[
				typeof(BaseEntity<>),
				typeof(BaseDto<>),
				typeof(IRequest),
				typeof(JobTriggerAttribute),
				typeof(IJob),
				typeof(DbContext),
				typeof(RelationalQueryableExtensions)
			];

			foreach (Type productType in productTypes)
			{
				assemblyPaths.Add(productType.Assembly.Location);
			}

			return assemblyPaths
				.Select(path => MetadataReference.CreateFromFile(path))
				.ToImmutableArray<MetadataReference>();
		}
	}
}
