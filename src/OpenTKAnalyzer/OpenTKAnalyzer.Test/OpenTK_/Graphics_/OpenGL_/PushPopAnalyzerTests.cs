using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class PushPopAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new PushPopAnalyzer();

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
		GL.PushAttrib(AttribMask.AllAttribBits);
		GL.PushMatrix();
		GL.PushClientAttrib(ClientAttribMask.ClientAllAttribBits);

		GL.PopAttrib();
		GL.PopClientAttrib();
		GL.PopMatrix();
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
		GL.PushMatrix();
		GL.PushAttrib(AttribMask.AllAttribBits);
		GL.PushMatrix();
		GL.PushName(1);

		GL.PopMatrix();
		GL.PopAttrib();
		GL.PopMatrix();
		GL.PopClientAttrib();
		GL.PopMatrix();
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = PushPopAnalyzer.DiagnosticId,
					Message = "Missing GL.PopMatrix.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 3)
				},
				new DiagnosticResult
				{
					Id = PushPopAnalyzer.DiagnosticId,
					Message = "Missing GL.PopName.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 3)
				},
				new DiagnosticResult
				{
					Id = PushPopAnalyzer.DiagnosticId,
					Message = "Missing GL.PushMatrix.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 3)
				},
				new DiagnosticResult
				{
					Id = PushPopAnalyzer.DiagnosticId,
					Message = "Missing GL.PushClientAttrib.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 3)
				},
				new DiagnosticResult
				{
					Id = PushPopAnalyzer.DiagnosticId,
					Message = "Missing GL.PushMatrix.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 15, 3)
				});
		}
	}
}