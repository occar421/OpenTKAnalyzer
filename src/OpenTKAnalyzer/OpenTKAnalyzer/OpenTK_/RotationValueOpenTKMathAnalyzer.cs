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
using OpenTK;

namespace OpenTKAnalyzer.OpenTK_
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	class RotationValueOpenTKMathAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "RotatinoValueOpenTKMath";

		private const string Title = "Rotation value(OpenTK Math)";
		private const string MessageFormat = "{0} accepts {1} values.";
		private const string Description = "Warm on literal in argument seems invalid style(radian or degree).";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK);

		private const string RadianString = "radian";
		private const string DegreeString = "degree";

		internal static DiagnosticDescriptor WarningRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: MessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description);
		internal static DiagnosticDescriptor InfoRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: MessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(WarningRule, InfoRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context)
		{
			var invotation = context.Node as InvocationExpressionSyntax;

			// MathHelper
			if (invotation.GetFirstToken().ValueText == nameof(MathHelper))
			{
				// check method
				switch (invotation.Expression.GetLastToken().ValueText)
				{
					default:
						break;
				}
				return;
			}

			// Matrix2, Matrix3x4 etc...
			if (invotation.GetFirstToken().ValueText.StartsWith("Matrix"))
			{
				// check method
				switch (invotation.Expression.GetLastToken().ValueText)
				{
					default:
						break;
				}
				return;
			}
		}
	}
}
