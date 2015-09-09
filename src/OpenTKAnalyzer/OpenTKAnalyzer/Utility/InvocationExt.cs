using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenTKAnalyzer.Utility
{
	public static class InvocationExt
	{
		public static string GetMethodName(this InvocationExpressionSyntax invocation)
		{
			return invocation.Expression.WithoutTrivia().ToFullString();
		}

		public static ExpressionSyntax GetNthArgumentExpression(this InvocationExpressionSyntax invocation, int n)
		{
			if (n < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(n), nameof(n) + " >= 0");
			}

			return invocation?.ArgumentList.Arguments.Skip(n).FirstOrDefault()?.Expression;
		}
	}
}
