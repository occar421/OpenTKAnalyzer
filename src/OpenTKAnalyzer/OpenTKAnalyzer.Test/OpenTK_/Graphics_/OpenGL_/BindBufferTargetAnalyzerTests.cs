using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class BindBufferTargetAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new BindBufferTargetAnalyzer();

		[TestMethod]
		public void EmptyCode()
		{
			var test0Source = @"";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void SimpleNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	int buffer = GL.GenBuffer();
	void Method1()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
	void Method2()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void BufferTargetLessNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.BufferTarget;
class Class1
{
	int buffer = GL.GenBuffer();
	void Method1()
	{
		GL.BindBuffer(ArrayBuffer, buffer);
		GL.BindBuffer(ArrayBuffer, 0);
	}
	void Method2()
	{
		GL.BindBuffer(ArrayBuffer, buffer);
		GL.BindBuffer(ArrayBuffer, 0);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void SimpleIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	int buffer = GL.GenBuffer();
	void Method1()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}
	void Method2()
	{
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer);
		GL.BindBuffer(BufferTarget.ArrayBuffer, 5);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "The variable \"buffer\" is used in multiple buffer targets (BufferTarget.ArrayBuffer, BufferTarget.ElementArrayBuffer).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 3)
				},
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "The variable \"buffer\" is used in multiple buffer targets (BufferTarget.ArrayBuffer, BufferTarget.ElementArrayBuffer).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 3)
				},
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "GL.BindBuffer accepts variable or 0 on 2nd argument.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 43)
				});
		}

		[TestMethod]
		public void BufferTargetLessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.BufferTarget;
class Class1
{
	int buffer = GL.GenBuffer();
	void Method1()
	{
		GL.BindBuffer(ArrayBuffer, buffer);
		GL.BindBuffer(ArrayBuffer, 0);
	}
	void Method2()
	{
		GL.BindBuffer(ElementArrayBuffer, buffer);
		GL.BindBuffer(ArrayBuffer, 5);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "The variable \"buffer\" is used in multiple buffer targets (BufferTarget.ArrayBuffer, BufferTarget.ElementArrayBuffer).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 3)
				},
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "The variable \"buffer\" is used in multiple buffer targets (BufferTarget.ArrayBuffer, BufferTarget.ElementArrayBuffer).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 3)
				},
				new DiagnosticResult()
				{
					Id = BindBufferTargetAnalyzer.DiagnosticId,
					Message = "GL.BindBuffer accepts variable or 0 on 2nd argument.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 30)
				});
		}
	}
}