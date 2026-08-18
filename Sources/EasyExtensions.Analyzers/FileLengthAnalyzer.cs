using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace EasyExtensions.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FileLengthAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			[DiagnosticDescriptors.FileTooLong];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
		}

		private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			AnalyzerConfigOptions options = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
			int maxLines = FileLengthOptions.GetMaxLines(options);
			SourceText sourceText = context.Tree.GetText(context.CancellationToken);
			SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
			bool[] codeLines = new bool[sourceText.Lines.Count];

			foreach (SyntaxToken token in root.DescendantTokens(descendIntoTrivia: false))
			{
				if (!token.IsMissing)
				{
					MarkLines(token.Span, sourceText.Lines, codeLines);
				}
			}

			foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: true))
			{
				if (trivia.GetStructure() is DirectiveTriviaSyntax)
				{
					MarkLines(trivia.Span, sourceText.Lines, codeLines);
				}
			}

			int codeLineCount = 0;
			int firstExcessLine = -1;

			for (int lineIndex = 0; lineIndex < codeLines.Length; lineIndex++)
			{
				if (!codeLines[lineIndex])
				{
					continue;
				}

				codeLineCount++;

				if (codeLineCount == maxLines + 1)
				{
					firstExcessLine = lineIndex;
				}
			}

			if (codeLineCount <= maxLines)
			{
				return;
			}

			TextSpan diagnosticSpan = sourceText.Lines[firstExcessLine].Span;
			Location location = Location.Create(context.Tree, diagnosticSpan);
			Diagnostic diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.FileTooLong,
				location,
				codeLineCount,
				maxLines);

			context.ReportDiagnostic(diagnostic);
		}

		private static void MarkLines(TextSpan span, TextLineCollection sourceLines, bool[] codeLines)
		{
			if (span.IsEmpty)
			{
				return;
			}

			int startLine = sourceLines.GetLineFromPosition(span.Start).LineNumber;
			int endLine = sourceLines.GetLineFromPosition(span.End - 1).LineNumber;

			for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
			{
				codeLines[lineIndex] = true;
			}
		}
	}
}
