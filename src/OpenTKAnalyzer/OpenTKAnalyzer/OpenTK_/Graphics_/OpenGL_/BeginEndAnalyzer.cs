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
	class BeginEndAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "BeginEnd";

		private const string Title = "GL.Begin and GL.End comformity";
		private const string MessageFormat = "Missing {0}.";
		private const string Description = "";
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
			// filtering
			if (context.CodeBlock.Ancestors().OfType<UsingDirectiveSyntax>()
				.Where(u => u.Name.NormalizeWhitespace().ToFullString() ==
					nameof(OpenTK) + "." + nameof(OpenTK.Graphics) + "." + nameof(OpenTK.Graphics.OpenGL)).Any())
			{
				return;
			}

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

			private struct Pair
			{
				internal MemberAccessExpressionSyntax Syntax { get; }
				internal string OperationName { get; }

				public Pair(MemberAccessExpressionSyntax syntax, string name)
				{
					Syntax = syntax;
					OperationName = name;
				}
			}

			public override void VisitBlock(BlockSyntax node)
			{
				var statements = node.ChildNodes().OfType<ExpressionStatementSyntax>();
				var invocations = statements.Select(s => s.Expression).OfType<InvocationExpressionSyntax>();
				var glOperators = invocations.Select(i => i.Expression).OfType<MemberAccessExpressionSyntax>()
					.Where(m => m.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Where(s => s.GetFirstToken().Text == nameof(GL));
				var beginEnds = glOperators.Select(g => new Pair(g, g.ChildNodes().Skip(1).First().GetFirstToken().Text))
					.Where(p => p.OperationName == nameof(GL.Begin) || p.OperationName == nameof(GL.End));

				// filtering
				if (!beginEnds.Any())
				{
					base.VisitBlock(node);
				}

				// block contains GL.Begin() or GL.End()
				var counter = 0;
				Location prevBeginLocation = null;
				foreach (var pair in beginEnds)
				{
					if (pair.OperationName == nameof(GL.Begin))
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
						prevBeginLocation = pair.Syntax.Parent.GetLocation();
					}
					else if (pair.OperationName == nameof(GL.End))
					{
						counter--;
						if (counter < 0)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: pair.Syntax.Parent.GetLocation(),
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
