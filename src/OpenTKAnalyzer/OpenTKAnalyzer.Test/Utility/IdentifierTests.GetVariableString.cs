using Microsoft.CodeAnalysis;
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
	public partial class IdentifierTests
	{
		[TestClass]
		public class GetVariableString
		{
			static SemanticModel semanticModel;
			static SyntaxNode syntaxRoot;
			static MethodDeclarationSyntax method1;
			static InvocationExpressionSyntax[] invocations;
			static string source = @"
namespace Namespace1
{
	class Class1
	{
		int a = 1;
		float[] b = new float[2];
		static string c = string.Empty;
		const decimal d = 10.3m;
		void Method1()
		{
			var a = new Class1();
			System.Func<int> c = () => 1;
			Method2(null);
			Method2(a);
			Method2(this.a);
			Method2(b);
			Method2(b[0]);
			Method2(b[1]);
			Method2(c);
			Method2(Class1.c);
			Method2(c());
			Method2(d);
		}
		void Method2(object anything)
		{
		}
	}
}
";

			[ClassInitialize]
			public static void ClassInitialize(TestContext context)
			{
				var tree = CSharpSyntaxTree.ParseText(source);
				var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
				var compilation = CSharpCompilation.Create(nameof(GetVariableString), new[] { tree }, new[] { mscorlib });
				semanticModel = compilation.GetSemanticModel(tree);
				syntaxRoot = tree.GetRoot();
				method1 = syntaxRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
				invocations = method1.Body.ChildNodes().OfType<ExpressionStatementSyntax>().Select(x => x.Expression as InvocationExpressionSyntax).ToArray();
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentNullException))]
			public void AllNull()
			{
				Identifier.GetVariableString(null, null);
			}

			[TestMethod]
			public void ArgumentNull()
			{
				Assert.AreEqual(null, Identifier.GetVariableString(null, semanticModel));
			}

			[TestMethod]
			public void ExpressionNullNode()
			{
				var nullNode = invocations[0].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual(null, Identifier.GetVariableString(nullNode, semanticModel));
			}

			[TestMethod]
			public void LocalVariableAgainstThisNode()
			{
				var localVariableNode = invocations[1].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("a", Identifier.GetVariableString(localVariableNode, semanticModel));
			}

			[TestMethod]
			public void ThisVariableNode()
			{
				var thisVariableNode = invocations[2].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("a", Identifier.GetVariableString(thisVariableNode, semanticModel));
			}

			[TestMethod]
			public void ArrayVariableNode()
			{
				var arrayVariableNode = invocations[3].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("b", Identifier.GetVariableString(arrayVariableNode, semanticModel));
			}

			[TestMethod]
			public void ArrayIndexedVariableNode()
			{
				var index0Node = invocations[4].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("b[0]", Identifier.GetVariableString(index0Node, semanticModel));

				var index1Node = invocations[5].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("b[1]", Identifier.GetVariableString(index1Node, semanticModel));
			}

			[TestMethod]
			public void LocalVariableAgainstStaticNode()
			{
				var variableNode = invocations[6].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("c", Identifier.GetVariableString(variableNode, semanticModel));
			}

			[TestMethod]
			public void StaticVariableWithClassNameNode()
			{
				var variableNode = invocations[7].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("c", Identifier.GetVariableString(variableNode, semanticModel));
			}

			[TestMethod]
			public void InvocationNode()
			{
				var invocationNode = invocations[8].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual(null, Identifier.GetVariableString(invocationNode, semanticModel));
			}

			[TestMethod]
			public void ConstNode()
			{
				var constNode = invocations[9].ArgumentList.Arguments.First().Expression;
				Assert.AreEqual("d", Identifier.GetVariableString(constNode, semanticModel));
			}
		}
	}
}