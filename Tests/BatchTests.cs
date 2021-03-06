using System.Collections.Generic;
using ListDiff;
using Xunit;

namespace ListDiffTests
{
	public class BatchTests
	{

		[Fact]
		public void OneRemoveAndOneAdd ()
		{
			var source = "abc";
			var dest = "b1c";
			// rem = "bc";

			var (rems, adds) = source.Diff (dest).GetBatchRemovesAndAdds ();

			Assert.Single (rems);
			Assert.Equal (0, rems[0].Index);
			Assert.Equal (1, rems[0].Count);

			Assert.Single (adds);
			Assert.Equal (1, adds[0].Index);
			Assert.Single (adds[0].Items);
		}

		[Fact]
		public void TwoRemoveAndTwoAddTwoRemoveTwoAdd ()
		{
			var source = "abcdghi";
			var dest = "ABcdCDi";
			// rem = "cdi";

			var (rems, adds) = source.Diff (dest).GetBatchRemovesAndAdds ();

			Assert.Equal (2, rems.Length);
			Assert.Equal (0, rems[0].Index);
			Assert.Equal (2, rems[0].Count);
			Assert.Equal (4, rems[1].Index);
			Assert.Equal (2, rems[1].Count);

			Assert.Equal (2, adds.Length);
			Assert.Equal (0, adds[0].Index);
			Assert.Equal (2, adds[0].Items.Length);
			Assert.Equal (4, adds[1].Index);
			Assert.Equal (2, adds[1].Items.Length);
		}
	}
}
