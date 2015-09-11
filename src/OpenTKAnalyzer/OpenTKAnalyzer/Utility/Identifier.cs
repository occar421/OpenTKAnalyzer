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
		/// <returns>indentity string : [Namespace].[Class].[MemberName] or ([Namespace].[Class].[Method]).[LocalVariableName]</returns>
		static public string GetVariableString(ExpressionSyntax expression, SemanticModel semanticModel)
		{
			if (semanticModel == null)
			{
				throw new ArgumentNullException(nameof(semanticModel));
			}
			if (expression == null)
			{
				return null;
			}
			var walker = new NamingWalker(semanticModel);
			walker.Visit(expression);
			return walker.Identity;
		}

		class NamingWalker : CSharpSyntaxWalker
		{
			SemanticModel semanticModel;
			string prefix = string.Empty;
			string id = string.Empty;
			string suffix = string.Empty;

			public string Identity => string.IsNullOrEmpty(id) ? null : prefix + id + suffix;

			public NamingWalker(SemanticModel semanticModel)
			{
				this.semanticModel = semanticModel;
			}

			public override void DefaultVisit(SyntaxNode node)
			{
				var symbol = semanticModel.GetSymbolInfo(node).Symbol;
				if (symbol?.Kind == SymbolKind.Local)
				{
					string methodName = symbol.ContainingSymbol.Name;

					var className = string.Empty;
					for (var cs = symbol.ContainingType; cs != null; cs = cs.ContainingType)
					{
						className = cs.Name + "." + className;
					}

					var namespaceName = string.Empty;
					for (var ns = symbol.ContainingNamespace; ns != null && ns.Name != string.Empty; ns = ns.ContainingNamespace)
					{
						namespaceName = ns.Name + "." + namespaceName;
					}
					id = $"({namespaceName}{className}{methodName}).{symbol.Name}";
				}
				else if (symbol?.Kind == SymbolKind.Method)
				{
					var str = symbol.ToString();
					id = str.Substring(0, str.Length - 2);
				}
				else
				{
					id = symbol?.ToString();
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				// no code here
			}

			public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
			{
				Visit(node.Expression);
				suffix = node.ArgumentList.WithoutTrivia().ToFullString() + suffix;
			}
		}
	}
}