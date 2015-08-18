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

			public override void VisitBlock(BlockSyntax node)
			{
				var statements = node.ChildNodes().OfType<ExpressionStatementSyntax>();
				var invocations = statements.Select(s => s.Expression).OfType<InvocationExpressionSyntax>();
				var glOperators = invocations.Select(i => i.Expression).OfType<MemberAccessExpressionSyntax>()
					.Where(m => m.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Where(s => s.GetFirstToken().Text == nameof(GL));

				// filtering
				if (!glOperators.Any())
				{
					base.VisitBlock(node);
				}

				// block contains GL.????()
				var counter = 0;
				foreach (var op in glOperators)
				{
					var opName = op.ChildNodes().Skip(1).First().GetFirstToken().Text;
					if (opName == nameof(GL.Begin))
					{
						counter++;
						if (counter > 1)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: op.Parent.GetLocation(),
								messageArgs: nameof(GL) + "." + nameof(GL.Begin)));
							counter = 1;
						}
					}
					else if (opName == nameof(GL.End))
					{
						counter--;
						if (counter < 0)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: op.Parent.GetLocation(),
								messageArgs: nameof(GL) + "." + nameof(GL.Begin)));
							counter = 0;
						}
					}
				}
				if (counter > 1)
				{
					Diagnostics.Add(Diagnostic.Create(
						descriptor: Rule,
						location: glOperators.Last().Parent.GetLocation(),
						messageArgs: nameof(GL) + "." + nameof(GL.End)));
				}

				base.VisitBlock(node);
			}
		}
	}
}
