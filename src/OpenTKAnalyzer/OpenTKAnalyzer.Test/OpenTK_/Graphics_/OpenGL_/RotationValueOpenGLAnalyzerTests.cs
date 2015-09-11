using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;
using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class RotationValueOpenGLAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new RotationValueOpenGLAnalyzer();

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
		GL.Rotate(10f, OpenTK.Vector3.One);
		GL.Rotate(10f, 1.0f, 1.0f, 1.0f);
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
		GL.Rotate(1.0f, OpenTK.Vector3.One);
		GL.Rotate(1.0f, 1.0f, 1.0f, 1.0f);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult()
				{
					Id = RotationValueOpenGLAnalyzer.DiagnosticId,
					Message = "GL.Rotate accepts degree value.",
					Severity = DiagnosticSeverity.Info,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 13)
				},
				new DiagnosticResult()
				{
					Id = RotationValueOpenGLAnalyzer.DiagnosticId,
					Message = "GL.Rotate accepts degree value.",
					Severity = DiagnosticSeverity.Info,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 13)
				});
		}
	}
}