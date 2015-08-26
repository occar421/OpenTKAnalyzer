using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LightParamAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "LightParam";

		private const string Title = "GL.Light param(third argument).";
		private const string MessageFormat = "{0} requires {1} in param(third argument).";
		private const string Description = "Error on used invalid argument on GL.Light param(third argument).";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
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

			var lights = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.Light));

			foreach (var lightOp in lights)
			{
				if (lightOp.ArgumentList.Arguments.Count == 3)
				{
					var secondExpression = lightOp.ArgumentList.Arguments.Skip(1).FirstOrDefault()?.ChildNodes()?.First();
					if (secondExpression == null)
					{
						continue;
					}
					var secondSymbol = context.SemanticModel.GetSymbolInfo(secondExpression).Symbol;
					if (secondSymbol?.OriginalDefinition?.ContainingType?.Name != nameof(LightParameter))
					{
						continue;
					}

					LightParameter secondEnum;
					if (Enum.TryParse(secondSymbol.Name, out secondEnum))
					{
						var thirdExpression = lightOp.ArgumentList.Arguments.Skip(2).FirstOrDefault()?.ChildNodes()?.First();
						if (thirdExpression == null)
						{
							continue;
						}
						var typeInfo = context.SemanticModel.GetTypeInfo(thirdExpression);
						var typeName = typeInfo.Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
						string[] correctTypeNames = null;
						switch (secondEnum)
						{
							// Color4, Vector4, int[4], float[4]
							// => Color4, Vector, int[], float[]
							case LightParameter.Ambient:
							case LightParameter.Diffuse:
							case LightParameter.Specular:
								correctTypeNames = new[] { nameof(OpenTK.Graphics.Color4), nameof(OpenTK.Vector4), "int[]", "float[]" };
								break;

							// Vector4, int[4], float[4]
							// => Vector4, int[], float[]
							case LightParameter.Position:
							case LightParameter.SpotDirection:
								correctTypeNames = new[] { nameof(OpenTK.Vector4), "int[]", "float[]" };
								break;

							// [0, 128]
							// => int, float
							case LightParameter.SpotExponent:
								correctTypeNames = new[] { "int", "float" };
								break;

							// [0, 90]
							// => int, float
							case LightParameter.SpotCutoff:
								correctTypeNames = new[] { "int", "float" };
								break;

							// int[3], float[3] (non negative)
							// => int[], float[]
							case LightParameter.ConstantAttenuation:
							case LightParameter.LinearAttenuation:
							case LightParameter.QuadraticAttenuation:
								correctTypeNames = new[] { "int[]", "float[]" };
								break;
						}
						if (correctTypeNames.Any(n => n == typeName))
						{
							continue;
						}
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: Rule,
							location: thirdExpression.GetLocation(),
							messageArgs: new[] { nameof(LightParameter) + "." + secondSymbol.Name, string.Join(", ", correctTypeNames) }));

					}
				}
			}
		}
	}
}