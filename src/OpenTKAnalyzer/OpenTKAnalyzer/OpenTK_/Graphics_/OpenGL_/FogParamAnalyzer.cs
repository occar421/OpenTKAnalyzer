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
	public class FogParamAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "FogParam";

		private const string Title = "GL.Fog param(second argument).";
		private const string MessageFormat = "{0} requires {1} in param(second argument).";
		private const string Description = "Error on used invalid argument on GL.Fog param(second argument).";
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

			var fogs = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.Fog));

			foreach (var fogOp in fogs)
			{
				if (fogOp.ArgumentList.Arguments.Count == 2)
				{
					var firstExpression = fogOp.ArgumentList.Arguments.FirstOrDefault()?.ChildNodes()?.First();
					if (firstExpression == null)
					{
						continue;
					}
					var firstSymbol = context.SemanticModel.GetSymbolInfo(firstExpression).Symbol;
					if (firstSymbol?.OriginalDefinition.ContainingType?.Name != nameof(FogParameter))
					{
						continue;
					}

					FogParameter firstEnum;
					if (Enum.TryParse(firstSymbol.Name, out firstEnum))
					{
						string[] correctTypeNames = null;
						bool useFogMode = false;
						switch (firstEnum)
						{
							// *special*
							// FogMode > Linear, Exp, exp2 (int casted)
							case FogParameter.FogMode:
								correctTypeNames = new[] { nameof(FogMode.Linear), nameof(FogMode.Exp), nameof(FogMode.Exp2) };
								useFogMode = true;
								break;

							// int, float
							case FogParameter.FogDensity:
							case FogParameter.FogStart:
							case FogParameter.FogEnd:
							case FogParameter.FogIndex:
								correctTypeNames = new[] { "int", "float" };
								break;

							// int[], float[]
							case FogParameter.FogColor:
								correctTypeNames = new[] { "int[]", "float[]" };
								break;

							// *special*
							// FogMode > FogCoord, FragmentDepth (int casted)
							case FogParameter.FogCoordSrc:
								correctTypeNames = new[] { nameof(FogMode.FogCoord), nameof(FogMode.FragmentDepth) };
								useFogMode = true;
								break;
						}

						// *special*
						if (useFogMode)
						{
							var argumentExpression = fogOp.ArgumentList.Arguments.Skip(1).FirstOrDefault();
							var castExpression = argumentExpression?.ChildNodes()?.FirstOrDefault();

							Location location = castExpression.GetLocation();
							if (castExpression.IsKind(SyntaxKind.CastExpression) && castExpression.ChildNodes().FirstOrDefault()?.ToFullString() == "int")
							{
								var insideExpression = argumentExpression?.ChildNodes()?.FirstOrDefault()?.ChildNodes().Last();
								if (insideExpression == null)
								{
									continue;
								}
								location = insideExpression.GetLocation();
								var typeInfo = context.SemanticModel.GetTypeInfo(insideExpression);
								if (typeInfo.Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) == nameof(FogMode))
								{
                                    var elementName = insideExpression.DescendantNodesAndSelf().LastOrDefault()?.ToFullString();
									if (correctTypeNames.Any(n => n == elementName))
									{
										continue;
									}
								}
							}
							context.ReportDiagnostic(Diagnostic.Create(
								descriptor: Rule,
								location: location,
								messageArgs: new[] { nameof(FogParameter) + "." + firstSymbol.Name, "int casted " + string.Join(", ", correctTypeNames.Select(n => nameof(FogMode) + "." + n)) }));
						}
						else
						{
							var secondExpression = fogOp.ArgumentList.Arguments.Skip(1).FirstOrDefault()?.ChildNodes()?.First();
							if (secondExpression == null)
							{
								continue;
							}
							var typeInfo = context.SemanticModel.GetTypeInfo(secondExpression);
							var typeName = typeInfo.Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);


							if (correctTypeNames.Any(n => n == typeName))
							{
								continue;
							}
							context.ReportDiagnostic(Diagnostic.Create(
								descriptor: Rule,
								location: secondExpression.GetLocation(),
								messageArgs: new[] { nameof(FogParameter) + "." + firstSymbol.Name, string.Join(", ", correctTypeNames) }));
						}
					}
				}
			}
		}
	}
}
