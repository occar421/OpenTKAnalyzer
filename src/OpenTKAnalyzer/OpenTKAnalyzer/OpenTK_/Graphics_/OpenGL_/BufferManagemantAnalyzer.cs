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
	public class BufferManagemantAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "BufferManagement";

		private const string Title = "Buffer management";
		private const string Description = "Check buffer usage.";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		internal static DiagnosticDescriptor NoConstantRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: "{0} accepts {1}.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor BufferTargetRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: "The variable \"{0}\" is used in multipul buffer targets ({1}).",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoConstantRule, BufferTargetRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSemanticModelAction(Analyze);
		}

		private static async void Analyze(SemanticModelAnalysisContext context)
		{
			var root = await context.SemanticModel.SyntaxTree.GetRootAsync();

			var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

			var bindBuffers = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.BindBuffer));
			var deleteBuffers = invocations.Where(i => i.Expression.WithoutTrivia().ToFullString() == nameof(GL) + "." + nameof(GL.DeleteBuffer));

			var syntaxValidBinds = bindBuffers.Where(b => b.ArgumentList.Arguments.Count == 2);
			var constantInvalidBinds = syntaxValidBinds.Select(b => b.ArgumentList.Arguments.Skip(1).First().ChildNodes().First())
				.OfType<LiteralExpressionSyntax>().Where(l => int.Parse(l.Token.ValueText) != 0);
			foreach (var bindLiteral in constantInvalidBinds)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NoConstantRule,
					location: bindLiteral.GetLocation(),
					messageArgs: new[] { nameof(GL) + "." + nameof(bindBuffers), "variable or 0" }));
			}

			var constantInvalidDeletes = deleteBuffers.Select(b => b.ArgumentList.Arguments.FirstOrDefault()?.ChildNodes().First())
				.OfType<LiteralExpressionSyntax>();
			foreach (var deleteLiteral in constantInvalidDeletes)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NoConstantRule,
					location: deleteLiteral.GetLocation(),
					messageArgs: new[] { nameof(GL) + "." + nameof(bindBuffers), "variable" }));
			}

			var variableBindGroups = syntaxValidBinds.Select(b => b.ArgumentList)
				.GroupBy(arg => context.SemanticModel.GetSymbolInfo(arg.Arguments.Skip(1).First().ChildNodes().First()).Symbol)
				.Where(g => g.Key != null); // <- constant on second argument
			foreach (var group in variableBindGroups)
			{
				var targets = group.GroupBy(g => context.SemanticModel.GetSymbolInfo(g.Arguments.First().ChildNodes().First())).Where(t => t.Key.Symbol != null);
				if (targets.Count() >= 2)
				{
					foreach (var invocation in group.Select(i => i.Parent))
					{
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: BufferTargetRule,
							location: invocation.GetLocation(),
							messageArgs: new[] { group.Key.Name, string.Join(", ", targets.Select(t => nameof(BufferTarget) + "." + t.Key.Symbol.Name)) }));
					}
				}
			}
		}
	}
}