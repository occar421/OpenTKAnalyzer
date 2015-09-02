using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace OpenTKAnalyzer.Utility.Tests
{
	public class NumericValueParserTests
	{
		[TestClass]
		public class TryParseFromExpression
		{
			[TestMethod]
			public void NormalScenario()
			{
				double result;
				ExpressionSyntax expression;

				// 1
				expression = LiteralExpression(NumericLiteralExpression, Literal(1));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, 1);

				// +1
				expression = PrefixUnaryExpression(UnaryPlusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, +1);

				// -1
				expression = PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, -1);

				// (1)
				expression = ParenthesizedExpression(LiteralExpression(NumericLiteralExpression, Literal(1)));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, 1);

				// (-1)
				expression = ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1))));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, -1);

				// -(-1)
				expression = PrefixUnaryExpression(UnaryMinusExpression, ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)))));
				Assert.IsTrue(NumericValueParser.TryParseFromExpression(expression, out result));
				Assert.AreEqual(result, 1);
			}

			[TestMethod]
			public void IncorrectScenario()
			{
				double result;
				ExpressionSyntax expression;

				// "num"
				expression = LiteralExpression(StringLiteralExpression, Literal("num"));
				Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result));

				// name
				expression = IdentifierName("name");
				Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result));

				// -name
				expression = PrefixUnaryExpression(UnaryMinusExpression, IdentifierName("name"));
				Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result));

				// (name)
				expression = ParenthesizedExpression(IdentifierName("name"));
				Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result));

				// 1+1
				expression = BinaryExpression(AddExpression, LiteralExpression(NumericLiteralExpression, Literal(1)), LiteralExpression(NumericLiteralExpression, Literal(1)));
                Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result));
			}
		}
	}
}