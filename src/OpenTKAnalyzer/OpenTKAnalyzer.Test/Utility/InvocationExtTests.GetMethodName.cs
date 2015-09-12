using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTKAnalyzer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKAnalyzer.Utility.Tests
{
	public partial class InvocationExtTests
	{
		[TestClass]
		public class GetMethodName
		{
			static InvocationExpressionSyntax[] invocations;
			static string source = @"using System;
class Class1
{
	void Method1()
	{
		ToString();
		Console.WriteLine();
		DateTime.Today.ToString();
	}
}";

			[ClassInitialize]
			public static void ClassInitialize(TestContext context)
			{
				var tree = CSharpSyntaxTree.ParseText(source);
				var root = tree.GetRoot();
				invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToArray();
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentNullException))]
			public void Null()
			{
				InvocationExt.GetMethodCallingName(null);
			}

			[TestMethod]
			public void NormalScenario()
			{
				Assert.AreEqual("ToString", invocations[0].GetMethodCallingName());
				Assert.AreEqual("Console.WriteLine", invocations[1].GetMethodCallingName());
				Assert.AreEqual("DateTime.Today.ToString", invocations[2].GetMethodCallingName());
			}
		}
	}
}