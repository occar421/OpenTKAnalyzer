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
		private const string MessageFormat = "{0} accepts {1} value.";
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

			// no arguments method filter
			if (!invotation.ArgumentList.Arguments.Any())
			{
				return;
			}

			double result;
			// MathHelper
			var baseName = invotation.GetFirstToken().ValueText;
			if (baseName == nameof(MathHelper))
			{
				// check method
				switch (invotation.Expression.GetLastToken().ValueText)
				{
					case nameof(MathHelper.DegreesToRadians):
						{
							var literal = invotation.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
							if (double.TryParse(literal?.Token.ValueText, out result))
							{
								// perhaps degree value under 2PI is incorrect
								if (Math.Abs(result) <= 2 * Math.PI)
								{
									context.ReportDiagnostic(Diagnostic.Create(
										descriptor: InfoRule,
										location: literal.GetLocation(),
										messageArgs: new[]
										{
											nameof(MathHelper) + "." + nameof(MathHelper.DegreesToRadians),
											DegreeString
										}));
								}
							}
						}
						return;

					case nameof(MathHelper.RadiansToDegrees):
						{
							var literal = invotation.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
							if (double.TryParse(literal?.Token.ValueText, out result))
							{
								// radian value usually under 2PI
								if (Math.Abs(result) >= 2 * Math.PI)
								{
									context.ReportDiagnostic(Diagnostic.Create(
										descriptor: WarningRule,
										location: literal.GetLocation(),
										messageArgs: new[]
										{
											nameof(MathHelper) + "." + nameof(MathHelper.RadiansToDegrees),
											RadianString
										}));
								}
							}
						}
						return;
					default:
						return;
				}
			}

			// Matrix2, Matrix3x4 etc...
			if (baseName.StartsWith("Matrix"))
			{
				// check method
				var methodName = invotation.Expression.GetLastToken().ValueText;
				switch (methodName)
				{
					// Matrix? or Matrix?x$(? or $ is 2)
					case nameof(Matrix2.CreateRotation):
						{
							var literal = invotation.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
							if (double.TryParse(literal?.Token.ValueText, out result))
							{
								// radian value usually under 2PI
								if (Math.Abs(result) >= 2 * Math.PI)
								{
									context.ReportDiagnostic(Diagnostic.Create(
										descriptor: WarningRule,
										location: literal.GetLocation(),
										messageArgs: new[] { baseName + "." + methodName, RadianString }));
								}
							}
						}
						return;

					// Matrix? or Matrix?x$(? or $ is 3 or 4)
					case nameof(Matrix3.CreateFromAxisAngle):
					// Matrix4
					case nameof(Matrix4.Rotate):
						{
							// literal is in second argument
							var literal = invotation.ArgumentList.Arguments.Skip(1).FirstOrDefault()?.Expression as LiteralExpressionSyntax;
							if (double.TryParse(literal?.Token.ValueText, out result))
							{
								// radian value usually under 2PI
								if (Math.Abs(result) >= 2 * Math.PI)
								{
									context.ReportDiagnostic(Diagnostic.Create(
										descriptor: WarningRule,
										location: literal.GetLocation(),
										messageArgs: new[] { baseName + "." + methodName, RadianString }));
								}
							}
						}
						return;
				}

				// Matrix? or Matrix?x$(? or $ is 3 or 4)
				// CreateRotationX, CreateRotationY, CreateRotationZ
				if (methodName.StartsWith("CreateRotation") ||
					// Matrix4
					// RotateX, RotateY, RotateZ
					methodName.StartsWith("Rotate"))
				{
					var literal = invotation.ArgumentList.Arguments.First().Expression as LiteralExpressionSyntax;
					if (double.TryParse(literal?.Token.ValueText, out result))
					{
						// radian value usually under 2PI
						if (Math.Abs(result) >= 2 * Math.PI)
						{
							context.ReportDiagnostic(Diagnostic.Create(
								descriptor: WarningRule,
								location: literal.GetLocation(),
								messageArgs: new[] { baseName + "." + methodName, RadianString }));
						}
					}
					return;
				}
				return;
			}
		}
	}
}
