using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class BeginEndAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new BeginEndAnalyzer();

		[TestMethod]
		public void EmptyCode()
		{
			var test0Source = @"";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void NormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Begin(BeginMode.LineLoop);
		GL.End();
	}
	void Method2()
	{
		GL.Begin(PrimitiveType.Triangles);
		GL.End();
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void IncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Begin(BeginMode.LineLoop);
	}
	void Method2()
	{
		GL.End();
	}
	void Method3()
	{
		GL.Begin(PrimitiveType.TrianglesAdjacency);
		GL.Begin(BeginMode.Patches);
		GL.End();
		GL.End();
		GL.End();
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult()
				{
					Id = BeginEndAnalyzer.DiagnosticId,
					Message = "Missing GL.End.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 3)
				},
				new DiagnosticResult()
				{
					Id = BeginEndAnalyzer.DiagnosticId,
					Message = "Missing GL.Begin.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 3)
				},
				new DiagnosticResult()
				{
					Id = BeginEndAnalyzer.DiagnosticId,
					Message = "Missing GL.End.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 3)
				},
				new DiagnosticResult()
				{
					Id = BeginEndAnalyzer.DiagnosticId,
					Message = "Missing GL.Begin.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 17, 3)
				},
				new DiagnosticResult()
				{
					Id = BeginEndAnalyzer.DiagnosticId,
					Message = "Missing GL.Begin.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 18, 3)
				});
		}
	}
}