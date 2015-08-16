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

namespace OpenTKAnalyzer.OpenTK.Graphics.OpenGL
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	class BeginEndAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "BeginEnd";

		private const string Title = "GL.Begin and GL.End";
		private const string MessageFormat = "{0} is missing.";
		private const string Description = "";
		private const string Category = "OpenTKAnalyzer:OpenGL";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
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
			// TODO: syntax walker with block
		}
	}
}
