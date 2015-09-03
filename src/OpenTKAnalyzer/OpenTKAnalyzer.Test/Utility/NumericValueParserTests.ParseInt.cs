using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using System.Collections.Generic;
using System;

namespace OpenTKAnalyzer.Utility.Tests
{
	public partial class NumericValueParserTests
	{
		[TestClass]
		public class ParseInt
		{
			struct Pair
			{
				public int Correct { get; }
				public ExpressionSyntax Expression { get; }

				public Pair(int correctValue, ExpressionSyntax expression)
				{
					Correct = correctValue;
					Expression = expression;
				}
			}

			readonly IEnumerable<Pair> GoodSamples = new[] {
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
				new Pair(1, PrefixUnaryExpression(UnaryMinusExpression, ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(1)))))),
			};

			readonly IEnumerable<ExpressionSyntax> BadExpressions = new ExpressionSyntax[]
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
				BinaryExpression(AddExpression, LiteralExpression(NumericLiteralExpression, Literal(1)), LiteralExpression(NumericLiteralExpression, Literal(1))),

				// 2.3
				LiteralExpression(NumericLiteralExpression, Literal(2.3)),

				// 2.3f
				LiteralExpression(NumericLiteralExpression, Literal("2.3f", 2.3f)),

				// +2.3
				PrefixUnaryExpression(UnaryPlusExpression, LiteralExpression(NumericLiteralExpression, Literal(2.3))),

				// -2.3
				PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(2.3))),

				// (2.3)
				ParenthesizedExpression(LiteralExpression(NumericLiteralExpression, Literal(2.3))),
				
				// (-2.3)
				ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(2.3)))),

				// -(-2.3)
				PrefixUnaryExpression(UnaryMinusExpression, ParenthesizedExpression(PrefixUnaryExpression(UnaryMinusExpression, LiteralExpression(NumericLiteralExpression, Literal(2.3)))))
			};

			[TestMethod]
			public void TryParseFromExpression_NormalScenario()
			{
				foreach (var sample in GoodSamples)
				{
					int result;
					Assert.IsTrue(NumericValueParser.TryParseFromExpression(sample.Expression, out result), $"parse error on \"{sample.Expression.ToFullString()}\"");
					Assert.AreEqual(result, sample.Correct, $"expected {sample.Correct} but {result} | on \"{sample.Expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void TryParseFromExpression_IncorrectScenario()
			{
				foreach (var expression in BadExpressions)
				{
					int result;
					Assert.IsFalse(NumericValueParser.TryParseFromExpression(expression, out result), $"incorrect result : {result} | parse mistakenly passed on \"{expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void ParseFromExpressionIntOrNull_NormalScenario()
			{
				foreach (var sample in GoodSamples)
				{
					var result = NumericValueParser.ParseFromExpressionIntOrNull(sample.Expression);
					Assert.AreEqual(result, sample.Correct, $"expected {sample.Correct} but {result} | on \"{sample.Expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void ParseFromExpressionIntOrNull_IncorrectScenario()
			{
				foreach (var expression in BadExpressions)
				{
					var result = NumericValueParser.ParseFromExpressionIntOrNull(expression);
					Assert.IsNull(result, $"incorrect result : {result} | parse mistakenly passed on \"{expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void ParseFromExpressionInt_NormalScenario()
			{
				foreach (var sample in GoodSamples)
				{
					var result = NumericValueParser.ParseFromExpressionInt(sample.Expression);
					Assert.AreEqual(result, sample.Correct, $"expected {sample.Correct} but {result} | on \"{sample.Expression.ToFullString()}\"");
				}
			}

			[TestMethod]
			public void ParseFromExpressionInt_IncorrectScenario()
			{
				foreach (var expression in BadExpressions)
				{
					try
					{
						var result = NumericValueParser.ParseFromExpressionInt(expression);
						Assert.Fail($"incorrect result : {result} | parse mistakenly passed on \"{expression.ToFullString()}\"");
					}
					catch (FormatException ex)
					{
						// ok
					}
				}
			}
		}
	}
}