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
	public class BeginEndAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "BeginEnd";

		private const string Title = "GL.Begin and GL.End comformity";
		private const string MessageFormat = "Missing {0}.";
		private const string Description = "Warm on comformity of GL.Begin and GL.End seems not good.";
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
			context.RegisterCodeBlockAction(Analyze);
		}

		private static void Analyze(CodeBlockAnalysisContext context)
		{
			// block contains OpenTK.Graphics.OpenGL
			var walker = new BlockWalker();
			walker.Visit(context.CodeBlock);
			foreach (var diagnostic in walker.Diagnostics)
			{
				context.ReportDiagnostic(diagnostic);
			}
		}

		class BlockWalker : CSharpSyntaxWalker
		{
			internal List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

			public override void VisitBlock(BlockSyntax node)
			{
				var statements = node.ChildNodes().OfType<ExpressionStatementSyntax>();
				var invocations = statements.Select(s => s.Expression).OfType<InvocationExpressionSyntax>();
				var glOperators = invocations.Select(i => i.Expression).OfType<MemberAccessExpressionSyntax>()
					.Where(m => m.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Where(s => s.GetFirstToken().Text == nameof(GL));
				var beginEnds = glOperators.Select(g => Tuple.Create(g.Parent as InvocationExpressionSyntax, g.ChildNodes().Skip(1).FirstOrDefault()?.GetFirstToken().Text))
					.Where(p => p.Item2 == nameof(GL.Begin) || p.Item2 == nameof(GL.End));

				// filtering
				if (!beginEnds.Any())
				{
					base.VisitBlock(node);
					return;
				}

				// block contains GL.Begin() or GL.End()
				var counter = 0;
				Location prevBeginLocation = null;
				foreach (var pair in beginEnds)
				{
					if (pair.Item2 == nameof(GL.Begin))
					{
						counter++;
						if (counter > 1)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: prevBeginLocation,
								messageArgs: nameof(GL) + "." + nameof(GL.End)));
							counter = 1;
						}
						prevBeginLocation = pair.Item1.GetLocation();
					}
					else if (pair.Item2 == nameof(GL.End))
					{
						counter--;
						if (counter < 0)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: pair.Item1.GetLocation(),
								messageArgs: nameof(GL) + "." + nameof(GL.Begin)));
							counter = 0;
						}
					}
				}
				if (counter > 0)
				{
					Diagnostics.Add(Diagnostic.Create(
						descriptor: Rule,
						location: prevBeginLocation,
						messageArgs: nameof(GL) + "." + nameof(GL.End)));
				}

				base.VisitBlock(node);
			}
		}
	}
}
