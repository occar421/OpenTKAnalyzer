using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace OpenTKAnalyzer.Utility.Tests
{
	public partial class NumericValueParserTests
	{
		[TestClass]
		public class ParseDouble
		{
			[TestMethod]
			public void TryParseFromExpression_NormalScenario()
			{
				var samples = new[] {
					// 1
					new Pair(1, LiteralExpression(NumericLiteralExpression, Literal(1))),

					// +1
					new Pair(1, PrefixUnaryExpression(UnaryPlusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)))),

					// -1
					new Pair(-1, PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)))),

					// (1)
					new Pair(1, ParenthesizedExpression(LiteralExpression(NumericLiteralExpression, Literal(1)))),

					// (-1)
					new Pair(-1, ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1))))),

					// -(-1)
					new Pair(1, PrefixUnaryExpression(UnaryMinusExpression, ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1))))))
				};

				foreach (var sample in samples)
				{
					double result;
					Assert.IsTrue(NumericValueParser.TryParseFromExpression(sample.Expression, out result), $"parse error on \"{sample.Expression.ToFullString()}\"");
					Assert.AreEqual(result, sample.Correct, $"expected {sample.Correct} but {result} | on \"{sample.Expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void TryParseFromExpression_IncorrectScenario()
			{
				var samples = new ExpressionSyntax[]
				{
					// "num"
					LiteralExpression(StringLiteralExpression, Literal("num")),

					// name
					IdentifierName("name"),

					// -name
					PrefixUnaryExpression(UnaryMinusExpression, IdentifierName("name")),

					// (name)
					ParenthesizedExpression(IdentifierName("name")),

					// 1+1
					BinaryExpression(AddExpression, LiteralExpression(NumericLiteralExpression, Literal(1)), LiteralExpression(NumericLiteralExpression, Literal(1)))
				};

				foreach (var expression in samples)
				{
					double result;
					Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result), $"parse mistakenly passed on \"{expression.ToFullString()}\"");
				}
			}

			struct Pair
			{
				public double Correct { get; }
				public ExpressionSyntax Expression { get; }

				public Pair(double correctValue, ExpressionSyntax expression)
				{
					Correct = correctValue;
					Expression = expression;
				}
			}
		}
	}
}