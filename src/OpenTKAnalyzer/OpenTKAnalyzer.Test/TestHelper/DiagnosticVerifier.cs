using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using static System.Environment;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace OpenTKAnalyzer.TestHelper
{
	/// <summary>
	/// Superclass of all Unit Tests for DiagnosticAnalyzers
	/// </summary>
	public abstract class DiagnosticVerifier
	{
		private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
		private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
		private static readonly MetadataReference OpenTKReference = MetadataReference.CreateFromFile(typeof(OpenTK.GameWindow).Assembly.Location);

		internal static string DefaultFilePathPrefix = "Test";
		internal static string CSharpDefaultFileExt = "cs";
		internal static string VisualBasicDefaultExt = "vb";
		internal static string TestProjectName = "TestProject";

		/// <summary>
		/// Get the C# analyzer being tested
		/// </summary>
		protected virtual DiagnosticAnalyzer CSharpDiagnosticAnalyzer => null;

		/// <summary>
		/// Get the VB analyzer being tested
		/// </summary>
		protected virtual DiagnosticAnalyzer BasicDiagnosticAnalyzer => null;

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// Note: input a <see cref="DiagnosticResult"/> for each Diagnostic expected
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on</param>
		/// <param name="expected"><see cref="DiagnosticResult"/>s that should appear after the analyzer is run on the source</param>
		protected void VerifyCSharpDiagnostic(string source, params DiagnosticResult[] expected) => VerifyDiagnostics(new[] { source }, LanguageNames.CSharp, CSharpDiagnosticAnalyzer, expected);

		/// <summary>
		/// Called to test a VB DiagnosticAnalyzer when applied on the single inputted string as a source
		/// Note: input a <see cref="DiagnosticResult"/> for each Diagnostic expected
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on</param>
		/// <param name="expected"><see cref="DiagnosticResult"/>s that should appear after the analyzer is run on the source</param>
		protected void VerifyBasicDiagnostic(string source, params DiagnosticResult[] expected) => VerifyDiagnostics(new[] { source }, LanguageNames.VisualBasic, BasicDiagnosticAnalyzer, expected);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source
		/// Note: input a <see cref="DiagnosticResult"/> for each Diagnostic expected
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
		/// <param name="expected"><see cref="DiagnosticResult"/>s that should appear after the analyzer is run on the sources</param>
		protected void VerifyCSharpDiagnostic(string[] sources, params DiagnosticResult[] expected) => VerifyDiagnostics(sources, LanguageNames.CSharp, CSharpDiagnosticAnalyzer, expected);

		/// <summary>
		/// Called to test a VB DiagnosticAnalyzer when applied on the inputted strings as a source
		/// Note: input a <see cref="DiagnosticResult"/> for each Diagnostic expected
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
		/// <param name="expected"><see cref="DiagnosticResult"/>s that should appear after the analyzer is run on the sources</param>
		protected void VerifyBasicDiagnostic(string[] sources, params DiagnosticResult[] expected) => VerifyDiagnostics(sources, LanguageNames.VisualBasic, BasicDiagnosticAnalyzer, expected);

		protected void VerifyDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expected) => VerifyDiagnosticResults(GetSourtedDiagnostics(sources, language, analyzer), analyzer, expected);

		private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
		{
			var actualsArray = actualResults.ToArray();
			var expectedsArray = expectedResults;
			if (expectedsArray.Length != actualsArray.Length)
			{
				var diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzer, actualsArray) : "    NONE.";

				Assert.Fail($"Mismatch between number of diagnostics returned, expected \"{expectedsArray.Length}\" actual \"{actualsArray.Length}\"{NewLine}{NewLine}Diagnostics:{NewLine}{diagnosticsOutput}{NewLine}");
			}

			for (int i = 0; i < expectedsArray.Length; i++)
			{
				var actual = actualsArray[i];
				var expected = expectedsArray[i];

				if (expected.Line == -1 && expected.Column == -1)
				{
					if (actual.Location != Location.None)
					{
						Assert.Fail($"Expected:{NewLine}A project diagnostic with No location{NewLine}Actual:{NewLine}{FormatDiagnostics(analyzer, actual)}");
					}
				}
				else
				{
					VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Location);
				}

				if (actual.Id != expected.Id)
				{
					Assert.Fail($"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, actual)}{NewLine}");
				}

				if (actual.Severity != expected.Severity)
				{
					Assert.Fail($"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, actual)}{NewLine}");
				}

				if (actual.GetMessage() != expected.Message)
				{
					Assert.Fail($"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage()}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, actual)}{NewLine}");
				}
			}
		}

		private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location acturalLocation, DiagnosticResultLocation expectedLocation)
		{
			var actualSpan = acturalLocation.GetLineSpan();

			Assert.IsTrue(actualSpan.Path == expectedLocation.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expectedLocation.Path.Contains("Test.")),
				$"Expected diagnostic to be in file \"{expectedLocation.Path}\" was actually in file \"{actualSpan.Path}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, diagnostic)}{NewLine}");


			var actualLinePosition = actualSpan.StartLinePosition;
			if (actualLinePosition.Line > 0 && actualLinePosition.Line + 1 != expectedLocation.Line)
			{
				Assert.Fail($"Expected diagnostic to be on line \"{expectedLocation.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, diagnostic)}{NewLine}");
			}
			if (actualLinePosition.Character > 0 && actualLinePosition.Character + 1 != expectedLocation.Column)
			{
				Assert.Fail($"Expected diagnostic to start at column \"{expectedLocation.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"{NewLine}{NewLine}Diagnostic:{NewLine}    {FormatDiagnostics(analyzer, diagnostic)}{NewLine}");
			}
		}

		private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
		{
			return string.Join(",", diagnostics.Select(diagnostic =>
			{
				var builder = new StringBuilder();
				builder.AppendLine($"// {diagnostic}");

				var analyzerType = analyzer.GetType();
				foreach (var rule in analyzer.SupportedDiagnostics.Where(d => d?.Id == diagnostic.Id))
				{
					if (diagnostic.Location == Location.None)
					{
						builder.Append($"GetGlobalResult({analyzerType.Name}.{rule.Id})");
					}
					else
					{
						Assert.IsTrue(diagnostic.Location.IsInSource, $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostic}{NewLine}");

						var resultMethodName = diagnostic.Location.SourceTree.FilePath.EndsWith(".cs") ? "CSharp" : "Basic";
						var linePosition = diagnostic.Location.GetLineSpan().StartLinePosition;
						builder.Append($"Get{resultMethodName}ResultAt({linePosition.Line + 1}, {linePosition.Character + 1}, {analyzerType.Name}.{rule.Id})");
					}

					builder.AppendLine();
					break;
				}
				return builder.ToString();
			}));
		}

		private static Diagnostic[] GetSourtedDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer) => GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources, language));

		/// <summary>
		/// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
		/// The returned diagnostics are then ordered by location in the source document.
		/// </summary>
		/// <param name="analyzer">The analyzer to run on the documents</param>
		/// <param name="documents">The Documents that the analyzer will be run on</param>
		/// <returns>An <see cref="IEnumerable{Diagnostic}"/> that surfaced in the source code, sorted by Location</returns>
		protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents)
		{
			var projects = new HashSet<Project>();
			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var diagnostics = new List<Diagnostic>();
			foreach (var project in projects)
			{
				var complicationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
				var diags = complicationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
				foreach (var diag in diags)
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						diagnostics.Add(diag);
					}
					else
					{
						foreach (var document in documents)
						{
							var tree = document.GetSyntaxTreeAsync().Result;
							if (tree == diag.Location.SourceTree)
							{
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			var results = SortDiagnostics(diagnostics);
			diagnostics.Clear();
			return results;
		}

		private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics) => diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

		private static Document[] GetDocuments(string[] sources, string language)
		{
			if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
			{
				throw new ArgumentException("Unsupported Language", nameof(language));
			}

			var project = CreateProject(sources, language);
			var documents = project.Documents.ToArray();
			if (sources.Length != documents.Length)
			{
				throw new SystemException("Amount of sources did not match amount of Documents created");
			}

			return documents;
		}

		/// <summary>
		/// Create a Document from a string through creating a project that contains it.
		/// </summary>
		/// <param name="source">Classes in the form of a string</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A <see cref="Document"/> created from the source string</returns>
		protected static Document CreateDocument(string source, string language = LanguageNames.CSharp) => CreateProject(new[] { source }, language).Documents.First();

		private static Project CreateProject(string[] sources, string language)
		{
			var fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;
			var projectId = ProjectId.CreateNewId(TestProjectName);
			var solution = new AdhocWorkspace().CurrentSolution.AddProject(projectId, TestProjectName, TestProjectName, language)
				.AddMetadataReference(projectId, CorlibReference)
				.AddMetadataReference(projectId, SystemCoreReference)
				.AddMetadataReference(projectId, CSharpSymbolsReference)
				.AddMetadataReference(projectId, CodeAnalysisReference)
				.AddMetadataReference(projectId, OpenTKReference);

			for (int i = 0; i < sources.Length; i++)
			{
				var newFileName = DefaultFilePathPrefix + i + "." + fileExt;
				var documentId = DocumentId.CreateNewId(projectId, newFileName);
				solution = solution.AddDocument(documentId, newFileName, SourceText.From(sources[i]));
			}
			return solution.GetProject(projectId);
		}
	}
}
