using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class FogParamAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new FogParamAnalyzer();

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
	void Method1()
	{
		GL.Fog(FogParameter.FogColor, new[] { 1, 2.0f });
		GL.Fog(FogParameter.FogIndex, 0.1f);
		GL.Fog(FogParameter.FogCoordSrc, (int)FogMode.FragmentDepth);
		GL.Fog(FogParameter.FogMode, (int)FogMode.Exp);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void FogParameterLessNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.FogParameter;
class Class1
{
	void Method1()
	{
		GL.Fog(FogColor, new[] { 1, 2.0f });
		GL.Fog(FogIndex, 0.1f);
		GL.Fog(FogCoordSrc, (int)OpenTK.Graphics.OpenGL.FogMode.FragmentDepth);
		GL.Fog(FogParameter.FogMode, (int)OpenTK.Graphics.OpenGL.FogMode.Exp);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void FogModeLessNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.FogMode;
class Class1
{
	void Method1()
	{
		GL.Fog(FogParameter.FogColor, new[] { 1, 2.0f });
		GL.Fog(FogParameter.FogIndex, 0.1f);
		GL.Fog(FogParameter.FogCoordSrc, (int)FragmentDepth);
		GL.Fog(FogParameter.FogMode, (int)Exp);
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
	void Method1()
	{
		GL.Fog(FogParameter.FogColor, 1);
		GL.Fog(FogParameter.FogIndex, new[] { 1, 2 });
		GL.Fog(FogParameter.FogCoordSrc, 1);
		GL.Fog(FogParameter.FogCoordSrc, (int)2.0);
		GL.Fog(FogParameter.FogCoordSrc, (int)FogMode.Exp);
		GL.Fog(FogParameter.FogMode, 1);
		GL.Fog(FogParameter.FogMode, (int)2.0);
		GL.Fog(FogParameter.FogMode, (int)FogMode.FragmentDepth);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogColor requires int[], float[] in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 33)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogIndex requires int, float in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 33)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 36)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 41)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 41)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 32)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 37)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 37)
				});
		}

		[TestMethod]
		public void FogParameterLessIncorrectTest()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.FogParameter;
class Class1
{
	void Method1()
	{
		GL.Fog(FogColor, 1);
		GL.Fog(FogIndex, new[] { 1, 2 });
		GL.Fog(FogCoordSrc, 1);
		GL.Fog(FogCoordSrc, (int)2.0);
		GL.Fog(FogCoordSrc, (int)OpenTK.Graphics.OpenGL.FogMode.Exp);
		GL.Fog(FogParameter.FogMode, 1);
		GL.Fog(FogParameter.FogMode, (int)2.0);
		GL.Fog(FogParameter.FogMode, (int)OpenTK.Graphics.OpenGL.FogMode.FragmentDepth);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogColor requires int[], float[] in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 20)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogIndex requires int, float in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 20)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 23)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 28)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 28)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 32)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 37)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 37)
				});
		}

		[TestMethod]
		public void FogModeLessincorrectTest()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.FogMode;
class Class1
{
	void Method1()
	{
		GL.Fog(FogParameter.FogColor, 1);
		GL.Fog(FogParameter.FogIndex, new[] { 1, 2 });
		GL.Fog(FogParameter.FogCoordSrc, 1);
		GL.Fog(FogParameter.FogCoordSrc, (int)2.0);
		GL.Fog(FogParameter.FogCoordSrc, (int)Exp);
		GL.Fog(FogParameter.FogMode, 1);
		GL.Fog(FogParameter.FogMode, (int)2.0);
		GL.Fog(FogParameter.FogMode, (int)FragmentDepth);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogColor requires int[], float[] in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 33)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogIndex requires int, float in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 33)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 36)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 41)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogCoordSrc requires int casted FogMode.FogCoord, FogMode.FragmentDepth in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 41)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 32)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 37)
				},
				new DiagnosticResult
				{
					Id = FogParamAnalyzer.DiagnosticId,
					Message = "FogParameter.FogMode requires int casted FogMode.Linear, FogMode.Exp, FogMode.Exp2 in param(second argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 37)
				});
		}
	}
}