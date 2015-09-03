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
	public class BindBufferTargetAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "BindBufferTarget";

		private const string Title = "GL.BindBuffer target check";
		private const string NoConstantMessageFormat = nameof(GL) + "." + nameof(GL.BindBuffer) + " accepts variable or 0 on 2nd argument.";
		private const string TargetMessageFormat = "The variable \"{0}\" is used in multiple buffer targets ({1}).";
		private const string Description = "Check buffer target.";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		internal static DiagnosticDescriptor NoConstantRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: NoConstantMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor TargetRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: TargetMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoConstantRule, TargetRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSemanticModelAction(Analyze);
		}

		private static async void Analyze(SemanticModelAnalysisContext context)
		{
			var root = await context.SemanticModel.SyntaxTree.GetRootAsync();

			var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

			var bindBuffers = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.BindBuffer));

			var syntaxValidBinds = bindBuffers.Where(b => b.ArgumentList.Arguments.Count == 2);
			var constantInvalidBinds = syntaxValidBinds.Select(b => b.ArgumentList.Arguments.Last().Expression)
				.Where(e => { var r = NumericValueParser.ParseFromExpressionIntOrNull(e); return r.HasValue && r.Value != 0; }); // null(not constant) or 0 are valid so be false
			foreach (var bindLiteral in constantInvalidBinds)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NoConstantRule,
					location: bindLiteral.GetLocation()));
			}

			var variableBindGroups = syntaxValidBinds.Select(b => b.ArgumentList)
				.GroupBy(arg =>
				{
					var node = arg.Arguments.Skip(1).First().ChildNodes().First();
					if (node is ElementAccessExpressionSyntax)
					{
						var identifierSymbol = context.SemanticModel.GetSymbolInfo(node.ChildNodes().First()).Symbol;
						var index = node.ChildNodes().Skip(1).FirstOrDefault()?.WithoutTrivia();
						return identifierSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + index.ToFullString();
					}
					else if (node is LiteralExpressionSyntax)
					{
						return null;
					}
					else
					{
						return context.SemanticModel.GetSymbolInfo(node).Symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
					}
				})
				.Where(g => g.Key != null); // <- constant on second argument
			foreach (var group in variableBindGroups)
			{
				var targets = group.GroupBy(g => context.SemanticModel.GetSymbolInfo(g.Arguments.First().Expression)).Where(t => t.Key.Symbol != null);
				if (targets.Count() >= 2)
				{
					foreach (var invocation in group.Select(i => i.Parent))
					{
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: TargetRule,
							location: invocation.GetLocation(),
							messageArgs: new[] { group.Key.Split('.').Last(), string.Join(", ", targets.Select(t => nameof(BufferTarget) + "." + t.Key.Symbol.Name)) }));
					}
				}
			}
		}
	}
}