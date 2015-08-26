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
	public class EnableLightingAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "EnableLighting";

		private const string Title = "GL.Enable for lighting.";
		private const string MessageFormat = "Missing {0}.";
		private const string Description = "Warm on forget lighting enabling.";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);


		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: MessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
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
			var useLights = new bool[8];
			var lightLocations = new List<Location>[8];
			for (int i = 0; i < lightLocations.Length; i++)
			{
				lightLocations[i] = new List<Location>();
			}
			foreach (var lightOp in lights)
			{
				var expression = lightOp.ArgumentList.Arguments.FirstOrDefault()?.ChildNodes()?.First();
				if (expression == null)
				{
					continue;
				}
				var symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol?.OriginalDefinition as IFieldSymbol;
				if (symbol?.Type?.Name != nameof(LightName))
				{
					continue;
				}
				int number;
				if (symbol.Name.Length > 5 && int.TryParse(symbol.Name.Substring(5), out number))
				{
					useLights[number] = true;
					lightLocations[number].Add(lightOp.GetLocation());
				}
			}
			if (!useLights.Any(u => u))
			{
				return;
			}

			// need light enabling
			var enables = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.Enable));
			var enableLights = new bool[8];
			var enableWholeLighting = false;
			var enableLocations = new List<Location>();
			foreach (var enableOp in enables)
			{
				var expression = enableOp.ArgumentList.Arguments.FirstOrDefault()?.ChildNodes()?.First();
				if (expression == null)
				{
					continue;
				}
				var symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol?.OriginalDefinition as IFieldSymbol;
				if (symbol?.Type?.Name != nameof(EnableCap))
				{
					continue;
				}
				if (symbol.Name == nameof(EnableCap.Lighting))
				{
					enableWholeLighting = true;
					continue;
				}
				int number;
				if (symbol.Name.Length > 5 && int.TryParse(symbol.Name.Substring(5), out number))
				{
					enableLights[number] = true;
					enableLocations.Add(enableOp.GetLocation());
				}
			}
			for (int i = 0; i < enableLights.Length; i++)
			{
				if (useLights[i] && !enableLights[i])
				{
					foreach (var location in lightLocations[i])
					{
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: Rule,
							location: location,
							messageArgs: nameof(GL) + "." + nameof(GL.Enable) + "(" + nameof(EnableCap) + ".Light" + i + ")"));
					}
				}
			}
			if (!enableWholeLighting)
			{
				foreach (var location in enableLocations)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: Rule,
						location: location,
						messageArgs: nameof(GL) + "." + nameof(GL.Enable) + "(" + nameof(EnableCap) + "." + nameof(EnableCap.Lighting) + ")"));
				}
			}
		}
	}
}
