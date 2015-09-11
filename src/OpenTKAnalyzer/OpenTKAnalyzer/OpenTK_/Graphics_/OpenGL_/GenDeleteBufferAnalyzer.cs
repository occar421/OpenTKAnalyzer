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
	public class GenDeleteBufferAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "GenerateDeleteBuffer";

		private const string Title = "GL.GenBuffer and GL.DeleteBuffer usage";
		private const string NoConstantMessageFormat = "{0} accepts variable.";
		private const string NotUsedMessageFormat = "Generated buffer by this " + nameof(GL) + "." + nameof(GL.GenBuffer) + " is not used.";
		private const string BuffersNumberMessageFormat = "{0} accepts larger than 0 value on 1st argument.";
		private const string GenDeleteOneBufferMessageFormat = "First argument must be 1 when second argument is variable of int or uint.";
		private const string DuplexMessageFormat = "The variable \"{0}\" is used in multiple {1}.";
		private const string LackGenBuffersMessageFormat = "The variable \"{0}\" is used deleting but not generating.";
		private const string LackDeleteBuffersMessageFormat = "The variable \"{0}\" is used generating but not deleting.";
		private const string GenDeleteNumberOfBuffersMessageFormat = nameof(GL) + "." + nameof(GL.GenBuffers) + " and " + nameof(GL) + "." + nameof(GL.DeleteBuffers) + " of the variable \"{0}\" may have different number of buffer.";
		private const string Description = "Analyzer for GL.GenBuffer and GL.DeleteBuffer.";
		private const string Category = nameof(OpenTKAnalyzer) + ":" + nameof(OpenTK.Graphics.OpenGL);

		internal static DiagnosticDescriptor NoConstantRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: NoConstantMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor NotUsedRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: NotUsedMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor BuffersNumberRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: BuffersNumberMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor GenDeleteOneBufferRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: GenDeleteOneBufferMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor DuplexBuffersRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: DuplexMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor LackGenBuffersRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: LackGenBuffersMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor LackDeleteBuffersRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: LackDeleteBuffersMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);

		internal static DiagnosticDescriptor GenDeleteNumberOfBuffersRule = new DiagnosticDescriptor(
			id: DiagnosticId,
			title: Title,
			messageFormat: GenDeleteNumberOfBuffersMessageFormat,
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: Description);


		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NoConstantRule, NotUsedRule, BuffersNumberRule, GenDeleteOneBufferRule, DuplexBuffersRule, LackGenBuffersRule, LackDeleteBuffersRule, GenDeleteNumberOfBuffersRule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSemanticModelAction(Analyze);
		}

		private static async void Analyze(SemanticModelAnalysisContext context)
		{
			var root = await context.SemanticModel.SyntaxTree.GetRootAsync();

			var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

			// check for GL.GenBuffer (single one)
			var genBufferOps = invocations.Where(i => i.GetMethodName() == nameof(GL) + "." + nameof(GL.GenBuffer));
			var notUsedGenBufferOps = genBufferOps.Where(g => !g.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression) && !g.Parent.IsKind(SyntaxKind.EqualsValueClause));
			foreach (var genOp in notUsedGenBufferOps)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NotUsedRule,
					location: genOp.GetLocation()));
			}

			// check for GL.GenBuffers (multiple one)
			var genBuffersOps = invocations.Where(i => i.GetMethodName() == nameof(GL) + "." + nameof(GL.GenBuffers));
			var firstArgConstGenBuffersOps = GetFirstArgConstInt(genBuffersOps);
			foreach (var genOp in FindNonNatural(firstArgConstGenBuffersOps))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: BuffersNumberRule,
					location: genOp.GetNthArgumentExpression(0).GetLocation(),
					messageArgs: nameof(GL) + "." + nameof(GL.GenBuffers)));
			}
			foreach (var genOp in FindNotOneInvalid(firstArgConstGenBuffersOps))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: GenDeleteOneBufferRule,
					location: genOp.GetNthArgumentExpression(0).GetLocation()));
			}

			// check for GL.DeleteBuffer (single one)
			var deleteBufferOps = invocations.Where(i => i.GetMethodName() == nameof(GL) + "." + nameof(GL.DeleteBuffer));
			var invalidDeleteBufferOps = GetFirstArgConstInt(deleteBufferOps).Where(o => o.Item2.HasValue);
			foreach (var deleteLiteral in invalidDeleteBufferOps)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: NoConstantRule,
					location: deleteLiteral.Item1.GetNthArgumentExpression(0).GetLocation(),
					messageArgs: nameof(GL) + "." + nameof(GL.DeleteBuffer)));
			}

			// check for GL.DeleteBuffers (multiple one)
			var deleteBuffersOps = invocations.Where(i => i.GetMethodName() == nameof(GL) + "." + nameof(GL.DeleteBuffers));
			var firstArgConstDeleteBuffersOps = GetFirstArgConstInt(deleteBuffersOps);
			foreach (var deleteOp in FindNonNatural(firstArgConstDeleteBuffersOps))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: BuffersNumberRule,
					location: deleteOp.GetNthArgumentExpression(0).GetLocation(),
					messageArgs: nameof(GL) + "." + nameof(GL.DeleteBuffers)));
			}
			foreach (var deleteOp in FindNotOneInvalid(firstArgConstDeleteBuffersOps))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: GenDeleteOneBufferRule,
					location: deleteOp.GetNthArgumentExpression(0).GetLocation()));
			}

			// GL.GenBuffers and GL.DeleteBuffers complex inspection
			var genOpGroupsById = GroupByVariableName(firstArgConstGenBuffersOps, context.SemanticModel);
			var deleteOpGroupsById = GroupByVariableName(firstArgConstDeleteBuffersOps, context.SemanticModel);

			// pick up all variable names
			var allKeys = Enumerable.Union(genOpGroupsById.Select(g => g.Key), deleteOpGroupsById.Select(d => d.Key));
			foreach (var key in allKeys)
			{
				var genOps = genOpGroupsById.FirstOrDefault(g => g.Key == key);
				var deleteOps = deleteOpGroupsById.FirstOrDefault(d => d.Key == key);
				bool isGenOpValid = true;
				bool isDeleteOpValid = true;

				if (genOps == null)
				{
					foreach (var delete in deleteOps)
					{
						var variableName = key.Split('.').Last();
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: LackGenBuffersRule,
							location: delete.Item1.GetLocation(),
							messageArgs: variableName));
					}
					isGenOpValid = false;
				}
				if (deleteOps == null)
				{
					foreach (var gen in genOps)
					{
						var variableName = key.Split('.').Last();
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: LackDeleteBuffersRule,
							location: gen.Item1.GetLocation(),
							messageArgs: variableName));
					}
					isDeleteOpValid = false;
				}

				if (isGenOpValid && genOps.Skip(1).Any()) // 2 or more gens with same variable
				{
					foreach (var gen in genOps)
					{
						var variableName = key.Split('.').Last();
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: DuplexBuffersRule,
							location: gen.Item1.GetLocation(),
							messageArgs: new[] { variableName, nameof(GL) + "." + nameof(GL.GenBuffers) }));
					}
					isGenOpValid = false;
				}
				if (isDeleteOpValid && deleteOps.Skip(1).Any()) // 2 or more deletes with same variable
				{
					foreach (var delete in deleteOps)
					{
						var variableName = key.Split('.').Last();
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: DuplexBuffersRule,
							location: delete.Item1.GetLocation(),
							messageArgs: new[] { variableName, nameof(GL) + "." + nameof(GL.DeleteBuffers) }));
					}
					isDeleteOpValid = false;
				}

				if (!isGenOpValid || !isDeleteOpValid)
				{
					continue;
				}

				// gen and delete are 1 to 1, but constant might different
				var genOp = genOps.First();
				var deleteOp = deleteOps.First();
				if (genOp.Item2.HasValue && deleteOp.Item2.HasValue)
				{
					if (genOp.Item2.Value != deleteOp.Item2.Value)
					{
						var variableName = key.Split('.').Last();
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: GenDeleteNumberOfBuffersRule,
							location: genOp.Item1.GetLocation(),
							messageArgs: variableName));
						context.ReportDiagnostic(Diagnostic.Create(
							descriptor: GenDeleteNumberOfBuffersRule,
							location: deleteOp.Item1.GetLocation(),
							messageArgs: variableName));
					}
				}
				// one is constant and another is variable so this usually wrong
				else if (genOp.Item2.HasValue || deleteOp.Item2.HasValue)
				{
					var variableName = key.Split('.').Last();
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: GenDeleteNumberOfBuffersRule,
						location: genOp.Item1.GetLocation(),
						messageArgs: variableName));
					context.ReportDiagnostic(Diagnostic.Create(
						descriptor: GenDeleteNumberOfBuffersRule,
						location: deleteOp.Item1.GetLocation(),
						messageArgs: variableName));
				}
			}
		}

		private static IEnumerable<Tuple<InvocationExpressionSyntax, int?>> GetFirstArgConstInt(IEnumerable<InvocationExpressionSyntax> invocations)
		{
			return invocations.Select(g => Tuple.Create(g, NumericValueParser.ParseFromExpressionIntOrNull(g.GetNthArgumentExpression(0))));
		}

		private static IEnumerable<InvocationExpressionSyntax> FindNonNatural(IEnumerable<Tuple<InvocationExpressionSyntax, int?>> ops)
		{
			return ops.Where(o => o.Item2.HasValue && o.Item2.Value < 1).Select(o => o.Item1);
		}

		private static IEnumerable<InvocationExpressionSyntax> FindNotOneInvalid(IEnumerable<Tuple<InvocationExpressionSyntax, int?>> ops)
		{
			return ops.Where(o => { var keyword = (o.Item1.GetNthArgumentExpression(1)?.Parent as ArgumentSyntax)?.RefOrOutKeyword; return keyword.HasValue && !keyword.Value.IsKind(SyntaxKind.None); })
				.Where(o => o.Item2.HasValue && o.Item2.Value != 1)
				.Select(o => o.Item1);
		}

		private static IEnumerable<IGrouping<string, Tuple<InvocationExpressionSyntax, int?>>> GroupByVariableName(IEnumerable<Tuple<InvocationExpressionSyntax, int?>> ops, SemanticModel semanticModel)
		{
			return ops.Select(o => new { Invocation = o.Item1, Const = o.Item2, Id = Identifier.GetVariableString(o.Item1.GetNthArgumentExpression(1), semanticModel) })
				.Where(o => !string.IsNullOrEmpty(o.Id))
				.GroupBy(g => g.Id, e => Tuple.Create(e.Invocation, e.Const));
		}
	}
}