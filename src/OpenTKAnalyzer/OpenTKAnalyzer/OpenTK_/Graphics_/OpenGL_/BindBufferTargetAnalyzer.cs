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

			var bindBuffers = invocations.Where(i => i.GetMethodCallingName() == nameof(GL) + "." + nameof(GL.BindBuffer));

			var syntaxValidBinds = bindBuffers.Where(b => b.ArgumentList.Arguments.Count == 2);
			var constantInvalidBinds = syntaxValidBinds.Select(b => b.GetArgumentExpressionAt(1))
				.Where(e => { var constant = context.SemanticModel.GetConstantValue(e); return constant.HasValue && !constant.Value.Equals(0); }); // null(not constant) or 0 are valid so be false
			foreach (var bindLiteral in constantInvalidBinds)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NoConstantRule,
					location: bindLiteral.GetLocation()));
			}

			var variableBindGroups = syntaxValidBinds.GroupBy(b => Identifier.GetVariableString(b.GetArgumentExpressionAt(1), context.SemanticModel))
				.Where(g => !string.IsNullOrEmpty(g.Key)); // <- constant on second argument
			foreach (var group in variableBindGroups)
			{
				var targets = group.GroupBy(b => context.SemanticModel.GetSymbolInfo(b.GetArgumentExpressionAt(0))).Where(t => t.Key.Symbol != null);
				if (targets.Skip(1).Any())
				{
					foreach (var invocation in group)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: TargetRule,
							location: invocation.GetLocation(),
							messageArgs: new[] { Identifier.GetSimpleName(group.Key), string.Join(", ", targets.Select(t => nameof(BufferTarget) + "." + t.Key.Symbol.Name)) }));
					}
				}
			}
		}
	}
}