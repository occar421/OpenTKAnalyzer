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
	class PushPopAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "PushPop";

		private const string Title = "GL.Push??? and GL.Pop??? comformity";
		private const string MessageFormat = "Missing {0}.";
		private const string Description = "Analyzer for GL.Push??? and GL.Push???.";
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

			private static IReadOnlyDictionary<string, int> router;
			private static IReadOnlyList<string> reverseRouter;

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

			static BlockWalker()
			{
				var dictionaryBuilder = ImmutableDictionary.CreateBuilder<string, int>();
				dictionaryBuilder.Add(nameof(GL.PushAttrib), 0);
				dictionaryBuilder.Add(nameof(GL.PopAttrib), 0);
				dictionaryBuilder.Add(nameof(GL.PushClientAttrib), 1);
				dictionaryBuilder.Add(nameof(GL.PopClientAttrib), 1);
				dictionaryBuilder.Add(nameof(GL.PushDebugGroup), 2);
				dictionaryBuilder.Add(nameof(GL.PopDebugGroup), 2);
				dictionaryBuilder.Add(nameof(GL.PushMatrix), 3);
				dictionaryBuilder.Add(nameof(GL.PopMatrix), 3);
				dictionaryBuilder.Add(nameof(GL.PushName), 4);
				dictionaryBuilder.Add(nameof(GL.PopName), 4);
				router = dictionaryBuilder.ToImmutableDictionary();

				var arrayBuilder = ImmutableArray.CreateBuilder<string>();
				arrayBuilder.Add(nameof(GL.PushAttrib).Substring(4));
				arrayBuilder.Add(nameof(GL.PushClientAttrib).Substring(4));
				arrayBuilder.Add(nameof(GL.PushDebugGroup).Substring(4));
				arrayBuilder.Add(nameof(GL.PushMatrix).Substring(4));
				arrayBuilder.Add(nameof(GL.PushName).Substring(4));
				reverseRouter = arrayBuilder.ToImmutableList();
			}

			public override void VisitBlock(BlockSyntax node)
			{
				var statements = node.ChildNodes().OfType<ExpressionStatementSyntax>();
				var invocations = statements.Select(s => s.Expression).OfType<InvocationExpressionSyntax>();
				var glOperators = invocations.Select(i => i.Expression).OfType<MemberAccessExpressionSyntax>()
					.Where(m => m.IsKind(SyntaxKind.SimpleMemberAccessExpression)).Where(s => s.GetFirstToken().Text == nameof(GL));
				var pushPops = glOperators.Select(g => new Pair(g, g.ChildNodes().Skip(1).First().GetFirstToken().Text))
					.Where(p => p.OperationName.StartsWith("Push") || p.OperationName.EndsWith("Pop"));

				// filtering
				if (!pushPops.Any())
				{
					base.VisitBlock(node);
				}

				// block contains GL.Push???() or GL.Pop???
				var counters = new int[reverseRouter.Count]; // new int[5]
				foreach (var pair in pushPops)
				{
					if (pair.OperationName.StartsWith("Push"))
					{
						counters[router[pair.OperationName]]++;
						if (counters[router[pair.OperationName]] > 1)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: pair.Syntax.Parent.GetLocation(),
								messageArgs: nameof(GL) + ".Pop" + reverseRouter[counters[router[pair.OperationName]]]));
							counters[router[pair.OperationName]] = 1;
						}
					}
					else if (pair.OperationName.StartsWith("Pop"))
					{
						counters[router[pair.OperationName]]--;
						if (counters[router[pair.OperationName]] < 0)
						{
							Diagnostics.Add(Diagnostic.Create(
								descriptor: Rule,
								location: pair.Syntax.Parent.GetLocation(),
								messageArgs: nameof(GL) + ".Push" + reverseRouter[counters[router[pair.OperationName]]]));
							counters[router[pair.OperationName]] = 0;
						}
					}
				}
				for (int i = 0; i < counters.Length; i++)
				{
					if (counters[i] > 0)
					{
						Diagnostics.Add(Diagnostic.Create(
							descriptor: Rule,
							location: pushPops.Last(p => p.OperationName.EndsWith(reverseRouter[i])).Syntax.Parent.GetLocation(),
							messageArgs: nameof(GL) + ".End" + reverseRouter[i]));
					}
				}

				base.VisitBlock(node);
			}
		}
	}
}
