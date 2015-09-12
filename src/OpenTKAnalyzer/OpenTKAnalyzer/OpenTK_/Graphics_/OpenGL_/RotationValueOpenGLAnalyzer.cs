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
using OpenTKAnalyzer.Utility;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RotationValueOpenGLAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "RotatinoValueOpenGL";

		private const string Title = "Rotation value(OpenGL)";
		private const string MessageFormat = nameof(GL) + "." + nameof(GL.Rotate) + " accepts degree value.";
		private const string Description = "Warm on literal in argument seems invalid style(radian or degree).";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: MessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context)
		{
			var invocation = context.Node as InvocationExpressionSyntax;

			// no arguments method filter
			if (!invocation.ArgumentList.Arguments.Any())
			{
				return;
			}
			if (invocation.GetFirstToken().ValueText == nameof(GL))
			{
				if (invocation.Expression.GetLastToken().ValueText == nameof(GL.Rotate))
				{
					DegreeValueAnalyze(context, invocation);
				}
			}
		}

		private static void DegreeValueAnalyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
		{
			double value;
			var argumentExpression = invocation.GetNthArgumentExpression(0);
			var result = context.SemanticModel.GetConstantValue(argumentExpression);
			if (double.TryParse(result.Value?.ToString(), out value))
			{
				// perhaps degree value under 2PI is incorrect
				if (Math.Abs(value) <= 2 * Math.PI)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: Rule,
						location: argumentExpression.GetLocation()));
				}
			}
		}

	}
}
