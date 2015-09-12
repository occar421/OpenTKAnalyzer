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
using OpenTKAnalyzer.Utility;

namespace OpenTKAnalyzer.OpenTK_
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class RotationValueOpenTKMathAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "RotatinoValueOpenTKMath";

		private const string Title = "Rotation value(OpenTK Math)";
		private const string NotRadianMessageFormat = "{0} accepts radian value.";
		private const string NotDegreeMessageFormat = "{0} accepts degree value.";
		private const string Description = "Warm on literal in argument seems invalid style(radian or degree).";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK);

		internal static DiagnosticDescriptor NotRadianRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: NotRadianMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description);
		internal static DiagnosticDescriptor NotDegreeRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: NotDegreeMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NotRadianRule, NotDegreeRule);

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

			// MathHelper
			var baseName = invocation.GetFirstToken().ValueText;
			if (baseName == nameof(MathHelper))
			{
				// check method
				switch (invocation.Expression.GetLastToken().ValueText)
				{
					case nameof(MathHelper.DegreesToRadians):
						DegreeValueAnalyze(context, invocation, 0, nameof(MathHelper) + "." + nameof(MathHelper.DegreesToRadians));
						break;

					case nameof(MathHelper.RadiansToDegrees):
						RadianValueAnalyze(context, invocation, 0, nameof(MathHelper) + "." + nameof(MathHelper.RadiansToDegrees));
						break;
				}
				return;
			}

			// Matrix2, Matrix3x4 etc...
			if (baseName.StartsWith("Matrix"))
			{
				// check method
				var methodName = invocation.Expression.GetLastToken().ValueText;
				switch (methodName)
				{
					// Matrix? or Matrix?x$(? or $ is 2)
					case nameof(Matrix2.CreateRotation):
						RadianValueAnalyze(context, invocation, 0, baseName + "." + methodName);
						return;

					// Matrix? or Matrix?x$(? or $ is 3 or 4)
					case nameof(Matrix3.CreateFromAxisAngle):
					// Matrix4
					case nameof(Matrix4.Rotate):
						// number is in second argument
						RadianValueAnalyze(context, invocation, 1, baseName + "." + methodName);
						return;
				}

				// Matrix? or Matrix?x$ Matrix?d Matrix?x$d(? or $ is 3 or 4)
				// CreateRotationX, CreateRotationY, CreateRotationZ
				if (methodName.StartsWith("CreateRotation") ||
					// Matrix4
					// RotateX, RotateY, RotateZ
					methodName.StartsWith("Rotate"))
				{
					RadianValueAnalyze(context, invocation, 0, baseName + "." + methodName);
				}
				return;
			}

			// Quartanion, Quartaniond
			if (baseName.StartsWith(nameof(Quaternion)))
			{
				var methodName = invocation.Expression.GetLastToken().ValueText;
				if (methodName == nameof(Quaternion.FromAxisAngle))
				{
					// number is in second argument
					RadianValueAnalyze(context, invocation, 1, baseName + "." + nameof(Quaternion.FromAxisAngle));
				}
			}
		}

		private static void DegreeValueAnalyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, int argumentIndex, string methodName)
		{
			double value;
			var argumentExpression = invocation.GetArgumentExpressionAt(argumentIndex);
			var result = context.SemanticModel.GetConstantValue(argumentExpression);
			if (double.TryParse(result.Value?.ToString(), out value))
			{
				// perhaps degree value under 2PI is incorrect
				if (Math.Abs(value) <= 2 * Math.PI)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: NotDegreeRule,
						location: argumentExpression.GetLocation(),
						messageArgs: methodName));
				}
			}
		}

		private static void RadianValueAnalyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, int argumentIndex, string methodName)
		{
			double value;
			var argumentExpression = invocation.GetArgumentExpressionAt(argumentIndex);
			var result = context.SemanticModel.GetConstantValue(argumentExpression);
			if (double.TryParse(result.Value?.ToString(), out value))
			{
				// radian value usually under 2PI
				if (Math.Abs(value) >= 2 * Math.PI)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: NotRadianRule,
						location: argumentExpression.GetLocation(),
						messageArgs: methodName));
				}
			}
		}
	}
}
