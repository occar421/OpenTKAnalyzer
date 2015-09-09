using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKAnalyzer.Utility
{
	static public class Identifier
	{
		/// <summary>
		/// Get identity string of variable.
		/// </summary>
		/// <param name="expression">expression node</param>
		/// <param name="semanticModel">semantic Model</param>
		/// <returns>indentity string</returns>
		static public string GetVariableString(ExpressionSyntax expression, SemanticModel semanticModel)
		{
			if (expression == null)
			{
				return null;
			}
			if (expression is ElementAccessExpressionSyntax)
			{
				var identifierSymbol = semanticModel.GetSymbolInfo(expression.ChildNodes().First()).Symbol;
				var index = expression.ChildNodes().Skip(1).FirstOrDefault()?.WithoutTrivia();
				return identifierSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + index.ToFullString();
			}
			if (expression is BinaryExpressionSyntax)
			{
				return null;
			}
			if (expression is PostfixUnaryExpressionSyntax)
			{
				return null;
			}
			if (NumericValueParser.ParseFromExpressionDoubleOrNull(expression as ExpressionSyntax).HasValue) // constant
			{
				return null;
			}
			if (expression is PrefixUnaryExpressionSyntax)
			{
				return null;
			}
			else
			{
				return semanticModel.GetSymbolInfo(expression).Symbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
			}
		}
	}
}