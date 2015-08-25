using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Environment;

namespace OpenTKAnalyzer.TestHelper
{
	/// <summary>
	/// Superclass of all Unit tests made for diagnostics with codefixes.
	/// Contains methods used to verify correctness of codefixes
	/// </summary>
	public abstract class CodeFixVerifier : DiagnosticVerifier
	{
		/// <summary>
		/// Get the C# codefix being tested
		/// </summary>
		protected abstract CodeFixProvider CSharpCodeFixProvider { get; }

		/// <summary>
		/// Get the VB codefix being tested
		/// </summary>
		protected abstract CodeFixProvider BasicCodeFixProvider { get; }

		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false) => VerifyFix(LanguageNames.CSharp, CSharpDiagnosticAnalyzer, CSharpCodeFixProvider, oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);

		/// <summary>
		/// Called to test a VB codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilationDiagnostics = false) => VerifyFix(LanguageNames.VisualBasic, BasicDiagnosticAnalyzer, BasicCodeFixProvider, oldSource, newSource, codeFixIndex, allowNewCompilationDiagnostics);

		private void VerifyFix(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics)
		{
			var document = CreateDocument(oldSource, language);
			var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
			var compilerDiagnostics = GetCompilerDiagnostics(document);

			for (int i = 0; i < analyzerDiagnostics.Length; i++)
			{
				var actions = new List<CodeAction>();
				var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
				codeFixProvider.RegisterCodeFixesAsync(context).Wait();
				if (!actions.Any())
				{
					break;
				}
				if (codeFixIndex != null)
				{
					document = ApplyFix(document, actions[codeFixIndex.Value]);
					break;
				}

				document = ApplyFix(document, actions[0]);
				analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));
				if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
				{
					document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
					newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

					Assert.Fail($"Fix introduced new compiler diagnostics:{NewLine}{string.Join(NewLine, newCompilerDiagnostics.Select(d => d.ToString()))}{NewLine}{NewLine}New document:{NewLine}{document.GetSyntaxRootAsync().Result.ToFullString()}{NewLine}");
				}

				if (!analyzerDiagnostics.Any())
				{
					break;
				}
			}

			Assert.AreEqual(newSource, GetStringFromDocument(document));
		}

		private static Document ApplyFix(Document document, CodeAction codeAction) => codeAction.GetOperationsAsync(CancellationToken.None).Result.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id);

		private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
		{
			var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
			var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

			var oldIndex = 0;
			var newIndex = 0;

			while (newIndex < newArray.Length)
			{
				if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
				{
					oldIndex++;
					newIndex++;
				}
				else
				{
					yield return newArray[newIndex++];
				}
			}
		}

		private static IEnumerable<Diagnostic> GetCompilerDiagnostics(Document document) => document.GetSemanticModelAsync().Result.GetDiagnostics();

		private static string GetStringFromDocument(Document document)
		{
			var simplifiedDocument = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
			return Formatter.Format(simplifiedDocument.GetSyntaxRootAsync().Result, Formatter.Annotation, simplifiedDocument.Project.Solution.Workspace).GetText().ToString();
		}
	}
}
