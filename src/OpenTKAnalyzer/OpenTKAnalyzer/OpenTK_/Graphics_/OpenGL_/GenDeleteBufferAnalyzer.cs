using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class GenDeleteBufferAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "GenerateDeleteBuffer";

		private const string Title = "GL.GenBuffer and GL.DeleteBuffer usage";
		private const string MessageFormat = "{0} accepts variable.";
		private const string Description = "Analyzer for GL.GenBuffer and GL.DeleteBuffer.";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: MessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSemanticModelAction(Analyze);
		}

		private static async void Analyze(SemanticModelAnalysisContext context)
		{
			var root = await context.SemanticModel.SyntaxTree.GetRootAsync();

			var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

			var deleteBuffers = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.DeleteBuffer));

			var constantInvalidDeletes = deleteBuffers.Select(b => b.ArgumentList.Arguments.FirstOrDefault()?.ChildNodes().First())
				.OfType<LiteralExpressionSyntax>();
			foreach (var deleteLiteral in constantInvalidDeletes)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: Rule,
					location: deleteLiteral.GetLocation(),
					messageArgs: nameof(GL) + "." + nameof(GL.DeleteBuffer)));
			}

			// TODO: GL.GenBuffers and GL.DeleteBuffers
		}
	}
}