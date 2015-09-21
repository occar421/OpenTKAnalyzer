using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class LightParamAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new LightParamAnalyzer();

		[TestMethod]
		public void EmptyCode()
		{
			var test0Source = @"";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void SimpleNormalScenario()
		{
			var test0Source = @"using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Light(LightName.Light0, LightParameter.Ambient, Color4.Black);
		GL.Light(LightName.Light1, LightParameter.Position, Vector4.One);
		GL.Light(LightName.Light2, LightParameter.SpotExponent, 90);
		GL.Light(LightName.Light2, LightParameter.SpotCutoff, 45);
		GL.Light(LightName.Light3, LightParameter.ConstantAttenuation, new[] { 1, 2, 3 });
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void LightParameterLessNormalScenario()
		{
			var test0Source = @"using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.LightParameter;
class Class1
{
	void Method1()
	{
		GL.Light(LightName.Light0, Ambient, Color4.Black);
		GL.Light(LightName.Light1, Position, Vector4.One);
		GL.Light(LightName.Light2, SpotExponent, 90);
		GL.Light(LightName.Light2, SpotCutoff, 45);
		GL.Light(LightName.Light3, ConstantAttenuation, new[] { 1, 2, 3 });
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void SimpleIncorrectScenario()
		{
			var test0Source = @"using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Light(LightName.Light0, LightParameter.Ambient, 1);
		GL.Light(LightName.Light1, LightParameter.Position, Color4.Black);
		GL.Light(LightName.Light2, LightParameter.SpotExponent, Vector4.One);
		GL.Light(LightName.Light2, LightParameter.SpotCutoff, new[] { 1, 2, 3, 4 });
		GL.Light(LightName.Light3, LightParameter.ConstantAttenuation, 1.0f);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.Ambient requires Color4, Vector4, int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 54)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.Position requires Vector4, int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 55)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.SpotExponent requires int, float in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 59)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.SpotCutoff requires int, float in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 57)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.ConstantAttenuation requires int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 66)
				});
		}

		[TestMethod]
		public void LightParameterLessIncorrectScenario()
		{
			var test0Source = @"using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.LightParameter;
class Class1
{
	void Method1()
	{
		GL.Light(LightName.Light0, Ambient, 1);
		GL.Light(LightName.Light1, Position, Color4.Black);
		GL.Light(LightName.Light2, SpotExponent, Vector4.One);
		GL.Light(LightName.Light2, SpotCutoff, new[] { 1, 2, 3, 4 });
		GL.Light(LightName.Light3, ConstantAttenuation, 1.0f);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.Ambient requires Color4, Vector4, int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 39)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.Position requires Vector4, int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 40)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.SpotExponent requires int, float in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 44)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.SpotCutoff requires int, float in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 42)
				},
				new DiagnosticResult
				{
					Id = LightParamAnalyzer.DiagnosticId,
					Message = "LightParameter.ConstantAttenuation requires int[], float[] in param(third argument).",
					Severity = DiagnosticSeverity.Error,
					Location = new DiagnosticResultLocation("Test0.cs", 13, 51)
				});
		}
	}
}