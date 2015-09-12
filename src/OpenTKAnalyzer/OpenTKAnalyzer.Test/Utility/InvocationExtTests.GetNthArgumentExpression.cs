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
		public class GetNthArgumentExpression
		{
			static InvocationExpressionSyntax invocation;
			static string source = @"class Class1
{
	void Method1()
	{
		System.Console.WriteLine(""str"", 1, 2, 3, 4);
	}
}";

			[ClassInitialize]
			public static void ClassInitialize(TestContext context)
			{
				var tree = CSharpSyntaxTree.ParseText(source);
				var root = tree.GetRoot();
				invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentOutOfRangeException))]
			public void NullAndMinus()
			{
				InvocationExt.GetArgumentExpressionAt(null, -1);
			}

			[TestMethod]
			public void InvocationNull()
			{
				Assert.AreEqual(null, InvocationExt.GetArgumentExpressionAt(null, 0));
			}

			[TestMethod]
			public void TakeArguments()
			{
				var first = invocation.GetArgumentExpressionAt(0);
				Assert.AreEqual("str", (first as LiteralExpressionSyntax)?.Token.ValueText);

				var second = invocation.GetArgumentExpressionAt(1);
				Assert.AreEqual("1", (second as LiteralExpressionSyntax)?.Token.ValueText);

				var third = invocation.GetArgumentExpressionAt(2);
				Assert.AreEqual("2", (third as LiteralExpressionSyntax)?.Token.ValueText);

				var fourth = invocation.GetArgumentExpressionAt(3);
				Assert.AreEqual("3", (fourth as LiteralExpressionSyntax)?.Token.ValueText);

				var fifth = invocation.GetArgumentExpressionAt(4);
				Assert.AreEqual("4", (fifth as LiteralExpressionSyntax)?.Token.ValueText);
			}

			[TestMethod]
			public void OutOfRangeIncorrectScenario()
			{
				var sixth = invocation.GetArgumentExpressionAt(5);
				Assert.AreEqual(null, sixth);
			}
		}
	}
}