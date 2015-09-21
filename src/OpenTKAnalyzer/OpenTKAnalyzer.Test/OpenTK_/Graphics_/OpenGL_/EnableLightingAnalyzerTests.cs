using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;

namespace OpenTKAnalyzer.OpenTK_.Graphics_.OpenGL_.Tests
{
	[TestClass]
	public class EnableLightingAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new EnableLightingAnalyzer();

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
		GL.Enable(EnableCap.Lighting);
		GL.Enable(EnableCap.Light0);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void EnableCapLessNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.EnableCap;
class Class1
{
	void Method1()
	{
		GL.Enable(Lighting);
		GL.Enable(Light0);
		GL.Enable(Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void LightNameLessNormalScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.LightName;
class Class1
{
	void Method1()
	{
		GL.Enable(EnableCap.Lighting);
		GL.Enable(EnableCap.Light0);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(Light0, LightParameter.SpotCutoff, 1);
		GL.Light(Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void SimpleLight0LessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Enable(EnableCap.Lighting);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Light0).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 3)
				});
		}

		[TestMethod]
		public void SimpleLightingLessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
class Class1
{
	void Method1()
	{
		GL.Enable(EnableCap.Light0);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 3)
				},
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 3)
				});
		}

		[TestMethod]
		public void EnableCapLessLight0LessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.EnableCap;
class Class1
{
	void Method1()
	{
		GL.Enable(Lighting);
		GL.Enable(Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Light0).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 3)
				});
		}

		[TestMethod]
		public void EnableCapLessLightingLessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.EnableCap;
class Class1
{
	void Method1()
	{
		GL.Enable(Light0);
		GL.Enable(Light1);
	}
	void Method2()
	{
		GL.Light(LightName.Light0, LightParameter.SpotCutoff, 1);
		GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 3)
				},
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 3)
				});
		}

		[TestMethod]
		public void LightNameLessLight0LessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.LightName;
class Class1
{
	void Method1()
	{
		GL.Enable(EnableCap.Lighting);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(Light0, LightParameter.SpotCutoff, 1);
		GL.Light(Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Light0).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 3)
				});
		}

		[TestMethod]
		public void LightNameLessLightingLessIncorrectScenario()
		{
			var test0Source = @"using OpenTK.Graphics.OpenGL;
using static OpenTK.Graphics.OpenGL.LightName;
class Class1
{
	void Method1()
	{
		GL.Enable(EnableCap.Light0);
		GL.Enable(EnableCap.Light1);
	}
	void Method2()
	{
		GL.Light(Light0, LightParameter.SpotCutoff, 1);
		GL.Light(Light1, LightParameter.ConstantAttenuation, 1);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 3)
				},
				new DiagnosticResult
				{
					Id = EnableLightingAnalyzer.DiagnosticId,
					Message = "Missing GL.Enable(EnableCap.Lighting).",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 8, 3)
				});
		}
	}
}