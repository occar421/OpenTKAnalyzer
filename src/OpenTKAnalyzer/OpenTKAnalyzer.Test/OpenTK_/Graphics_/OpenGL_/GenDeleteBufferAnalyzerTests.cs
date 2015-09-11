using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_;
using OpenTKAnalyzer.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class GenDeleteBufferAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new GenDeleteBufferAnalyzer();

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
	int field = GL.GenBuffer();
	void Method1()
	{
		int buf = GL.GenBuffer();
		int[] primary = new int[3];
		int[] secondory = new int[2];

		GL.GenBuffers(3, primary);
		GL.GenBuffers(secondory.Length, secondory);
		GL.GenBuffers(1, out buf);

		GL.DeleteBuffer(field);
		GL.DeleteBuffers(3, primary);
		GL.DeleteBuffers(secondory.Length, secondory);
		GL.DeleteBuffers(1, ref buf);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void Simple_IncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	int field;
	void Method1()
	{
		int buf;
		GL.GenBuffer();
		int[] primary = new int[3];

		GL.GenBuffers(2, out buf);
		GL.GenBuffers(0, primary);
		GL.DeleteBuffer(1);
		GL.DeleteBuffers(2, ref buf);
		GL.DeleteBuffers(0, primary);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "Generated buffer by this GL.GenBuffer is not used.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 3)
				},
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "First argument must be 1 when second argument is variable of int or uint.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 17)
				},
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.GenBuffers accepts larger than 0 value on 1st argument.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 17)
				},
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.DeleteBuffer accepts variable.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 19)
				},
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "First argument must be 1 when second argument is variable of int or uint.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 20)
				},
				new DiagnosticResult
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.DeleteBuffers accepts larger than 0 value on 1st argument.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 15, 20)
				});
		}

		[TestMethod]
		public void Complex_IncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		int first = 1, second = 2, third = 3, fourth = 4;
		int[] primary = new int[1], secondary = new int[2], tertiary = new int[3], quaternary = new int[4];

		GL.GenBuffers(1, out first);

		GL.DeleteBuffers(1, ref second);

		GL.GenBuffers(1, out third);
		GL.GenBuffers(1, out third);
		GL.DeleteBuffers(1, ref third);

		GL.GenBuffers(1, out fourth);
		GL.DeleteBuffers(1, ref fourth);
		GL.DeleteBuffers(1, ref fourth);

		GL.GenBuffers(1, primary); // ok
		GL.DeleteBuffers(1, primary); // ok

		GL.GenBuffers(2, secondary);
		GL.DeleteBuffers(3, secondary);

		int three = 3;
		GL.GenBuffers(3, tertiary);
		GL.DeleteBuffers(three, tertiary);

		int four = 4, cuatro = 4;
		GL.GenBuffers(four, quaternary); // cannot check
		GL.DeleteBuffers(cuatro, quaternary); // cannot check
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"first\" is used generating but not deleting.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"second\" is used deleting but not generating.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"third\" is used in multiple GL.GenBuffers.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"third\" is used in multiple GL.GenBuffers.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"fourth\" is used in multiple GL.DeleteBuffers.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 18, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "The variable \"fourth\" is used in multiple GL.DeleteBuffers.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 19, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.GenBuffers and GL.DeleteBuffers of the variable \"secondary\" may have different number of buffer.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 24, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.GenBuffers and GL.DeleteBuffers of the variable \"secondary\" may have different number of buffer.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 25, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.GenBuffers and GL.DeleteBuffers of the variable \"tertiary\" may have different number of buffer.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 28, 3)
				},
				new DiagnosticResult()
				{
					Id = GenDeleteBufferAnalyzer.DiagnosticId,
					Message = "GL.GenBuffers and GL.DeleteBuffers of the variable \"tertiary\" may have different number of buffer.",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 29, 3)
				});
		}
	}
}