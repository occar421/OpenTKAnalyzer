using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace OpenTKAnalyzer.Utility
{
	static public class NumericValueParser
	{
		static public bool TryParseFromExpression(ExpressionSyntax expression, out double result)
		{
			var parsed = ParseFromExpressionDoubleOrNull(expression);
			result = parsed.HasValue ? parsed.Value : double.NegativeInfinity;
			return parsed.HasValue;
		}

		static public double? ParseFromExpressionDoubleOrNull(ExpressionSyntax expression)
		{
			var walker = new DoubleExpressionWalker();
			walker.Visit(expression);
			return walker.Value;
		}

		static public double ParseFromExpressionDouble(ExpressionSyntax expression)
		{
			var result = ParseFromExpressionDoubleOrNull(expression);
			if (result.HasValue)
			{
				return result.Value;
			}
			else
			{
				throw new FormatException($"fail on {expression.WithoutTrivia().ToFullString()}");
			}
		}

		class DoubleExpressionWalker : CSharpSyntaxWalker
		{
			internal double? Value { get; private set; } = null;

			bool isValuePositive = true;

			// fail
			public override void DefaultVisit(SyntaxNode node)
			{
				return;
			}

			public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
			{
				switch (node.OperatorToken.ValueText)
				{
					case "+":
						break;
					case "-":
						isValuePositive = !isValuePositive;
						break;
					default:
						return;
				}
				base.Visit(node.Operand);
			}

			public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
			{
				base.Visit(node.Expression);
			}

			public override void VisitLiteralExpression(LiteralExpressionSyntax node)
			{
				if (node.IsKind(SyntaxKind.NumericLiteralExpression))
				{
					double result;
					if (double.TryParse(node.Token.ValueText, out result))
					{
						Value = (isValuePositive ? 1 : -1) * result;
					}
				}
			}
		}

		static public bool TryParseFromExpression(ExpressionSyntax expression, out int result)
		{
			var parsed = ParseFromExpressionIntOrNull(expression);
			result = parsed.HasValue ? parsed.Value : int.MinValue;
			return parsed.HasValue;
		}

		static public int? ParseFromExpressionIntOrNull(ExpressionSyntax expression)
		{
			var walker = new IntExpressionWalker();
			walker.Visit(expression);
			return walker.Value;
		}

		static public double ParseFromExpressionInt(ExpressionSyntax expression)
		{
			var result = ParseFromExpressionIntOrNull(expression);
			if (result.HasValue)
			{
				return result.Value;
			}
			else
			{
				throw new FormatException($"fail on {expression.WithoutTrivia().ToFullString()}");
			}
		}

		class IntExpressionWalker : CSharpSyntaxWalker
		{
			internal int? Value { get; private set; } = null;

			bool isValuePositive = true;

			// fail
			public override void DefaultVisit(SyntaxNode node)
			{
				return;
			}

			public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
			{
				switch (node.OperatorToken.ValueText)
				{
					case "+":
						break;
					case "-":
						isValuePositive = !isValuePositive;
						break;
					default:
						return;
				}
				base.Visit(node.Operand);
			}

			public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
			{
				base.Visit(node.Expression);
			}

			public override void VisitLiteralExpression(LiteralExpressionSyntax node)
			{
				if (node.IsKind(SyntaxKind.NumericLiteralExpression))
				{
					int result;
					if (int.TryParse(node.Token.ValueText, out result))
					{
						Value = (isValuePositive ? 1 : -1) * result;
					}
				}
			}
		}
	}
}