using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.TestHelper;
using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.OpenTK_.Tests
{
	[TestClass]
	public class RotationValueOpenTKMathAnalyzerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer CSharpDiagnosticAnalyzer => new RotationValueOpenTKMathAnalyzer();

		[TestMethod]
		public void EmptyCode()
		{
			var test0Source = @"";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void NormalScenario()
		{
			var test0Source = @"using OpenTK;
class Class1
{
	void Method1()
	{
		var a = 100;
		MathHelper.DegreesToRadians(60);
		MathHelper.DegreesToRadians(a);
		MathHelper.RadiansToDegrees(0.9f);

		Matrix2x3.CreateRotation(1.2f);
		Matrix2x3.CreateRotation(a);
		Matrix3.CreateFromAxisAngle(Vector3.One, 1f);
		Matrix3.CreateFromAxisAngle(Vector3.One, a);
		Matrix4d.CreateRotationY(3.4f);
		Matrix4d.CreateRotationY(a);
		Matrix4.Rotate(Vector3.One, 0.5f);
		Matrix4.Rotate(Vector3.One, a);

		Quaternion.FromAxisAngle(Vector3.UnitX, 0.6f);
		Quaternion.FromAxisAngle(Vector3.UnitX, a);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void IncorrectSenario()
		{
			var test0Source = @"using OpenTK;
class Class1
{
	void Method1()
	{
		MathHelper.DegreesToRadians(1f);
		MathHelper.RadiansToDegrees(10f);

		Matrix2x3.CreateRotation(10f);
		Matrix3.CreateFromAxisAngle(Vector3.One, 10f);
		Matrix4d.CreateRotationY(10f);
		Matrix4.Rotate(Vector3.One, 10f);

		Quaternion.FromAxisAngle(Vector3.UnitX, 10f);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "MathHelper.DegreesToRadians accepts degree value.",
					Severity = DiagnosticSeverity.Info,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "MathHelper.RadiansToDegrees accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix2x3.CreateRotation accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 28)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix3.CreateFromAxisAngle accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 44)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix4d.CreateRotationY accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 28)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix4.Rotate accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Quaternion.FromAxisAngle accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 43)
				});
		}

		[TestMethod]
		public void PrefixMinusNormalScenario()
		{
			var test0Source = @"using OpenTK;
class Class1
{
	void Method1()
	{
		var a = 100;
		MathHelper.DegreesToRadians(-60);
		MathHelper.DegreesToRadians(-a);
		MathHelper.RadiansToDegrees(-0.9f);

		Matrix2x3.CreateRotation(-1.2f);
		Matrix2x3.CreateRotation(-a);
		Matrix3.CreateFromAxisAngle(Vector3.One, -1f);
		Matrix3.CreateFromAxisAngle(Vector3.One, -a);
		Matrix4d.CreateRotationY(-3.4f);
		Matrix4d.CreateRotationY(-a);
		Matrix4.Rotate(Vector3.One, -0.5f);
		Matrix4.Rotate(Vector3.One, -a);

		Quaternion.FromAxisAngle(Vector3.UnitX, -0.6f);
		Quaternion.FromAxisAngle(Vector3.UnitX, -a);
	}
}";

			VerifyCSharpDiagnostic(test0Source);
		}

		[TestMethod]
		public void PrefixMinusIncorrectScenario()
		{
			var test0Source = @"using OpenTK;
class Class1
{
	void Method1()
	{
		MathHelper.DegreesToRadians(-1f);
		MathHelper.RadiansToDegrees(-10f);

		Matrix2x3.CreateRotation(-10f);
		Matrix3.CreateFromAxisAngle(Vector3.One, -10f);
		Matrix4d.CreateRotationY(-10f);
		Matrix4.Rotate(Vector3.One, -10f);

		Quaternion.FromAxisAngle(Vector3.UnitX, -10f);
	}
}";

			VerifyCSharpDiagnostic(test0Source,
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "MathHelper.DegreesToRadians accepts degree value.",
					Severity = DiagnosticSeverity.Info,
					Location = new DiagnosticResultLocation("Test0.cs", 6, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "MathHelper.RadiansToDegrees accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 7, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix2x3.CreateRotation accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 9, 28)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix3.CreateFromAxisAngle accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 10, 44)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix4d.CreateRotationY accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 11, 28)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Matrix4.Rotate accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 12, 31)
				},
				new DiagnosticResult
				{
					Id = RotationValueOpenTKMathAnalyzer.DiagnosticId,
					Message = "Quaternion.FromAxisAngle accepts radian value.",
					Severity = DiagnosticSeverity.Warning,
					Location = new DiagnosticResultLocation("Test0.cs", 14, 43)
				});
		}
	}
}