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

namespace OpenTKAnalyzer.Test.Utility
{
	public partial class IdentifierTests
	{
		[TestClass]
		public class GetSimpleName
		{
			[TestMethod]
			public void Null()
			{
				Assert.AreEqual(null, Identifier.GetSimpleName(null));
			}

			[TestMethod]
			public void LocalVariable()
			{
				Assert.AreEqual("foo", Identifier.GetSimpleName("(Namespace1.Class1.Method1).foo"));
			}

			[TestMethod]
			public void Field()
			{
				Assert.AreEqual("bar", Identifier.GetSimpleName("Namespace1.Class.bar"));
			}
		}
	}
}
