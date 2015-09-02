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
			var walker = new DoubleExpressWalker();
			walker.Visit(expression);
			result = walker.Value ?? double.NegativeInfinity;
			return walker.Value.HasValue;
		}

		class DoubleExpressWalker : CSharpSyntaxWalker
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
					Value = (isValuePositive ? 1 : -1) * double.Parse(node.Token.ValueText);
				}
			}
		}
	}
}