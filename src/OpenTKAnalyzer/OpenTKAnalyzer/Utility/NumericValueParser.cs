using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKAnalyzer.Utility
{
	static public class NumericValueParser
	{
		static public bool TryParseFromExpression(ExpressionSyntax expression, out double result)
		{
			var literalString = string.Empty;
			if (expression is PrefixUnaryExpressionSyntax)
			{
				var s = expression as PrefixUnaryExpressionSyntax;
				literalString = s.OperatorToken.ValueText + (s.Operand as LiteralExpressionSyntax)?.Token.ValueText;
			}
			else if (expression is LiteralExpressionSyntax)
			{
				var s = expression as LiteralExpressionSyntax;
				literalString = s.Token.ValueText;
			}
			return double.TryParse(literalString, out result);
		}
	}
}