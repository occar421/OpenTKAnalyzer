using Xunit;

namespace OpenTKAnalyzer.Test.OpenTK_
{
	public class HogeTest
	{
		[Fact]
		public void PassingTest()
		{
			Assert.Equal(4, 2 + 2);
		}

		[Fact]
		public void FailingTest()
		{
			Assert.Equal(5, 2 + 2);
		}
	}
}
