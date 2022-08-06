using System.Collections.Generic;
using ListDiff;
using Xunit;

namespace ListDiffTests
{
	public class Tests
	{
		[Theory]
		[InlineData ("", "", "")]
		[InlineData ("", "a", "+(a)")]
		[InlineData ("a", "", "-(a)")]
		[InlineData ("a", "a", "a")]
		[InlineData ("a", "b", "-(a)+(b)")]
		[InlineData ("ab", "ab", "ab")]
		[InlineData ("abc", "ab", "ab-(c)")]
		[InlineData ("ab", "abc", "ab+(c)")]
		[InlineData ("ab", "zab", "+(z)ab")]
		[InlineData ("ab", "b", "-(a)b")]
		[InlineData ("abc", "ac", "a-(b)c")]
		[InlineData ("abc", "a", "a-(b)-(c)")]
		[InlineData ("abc", "c", "-(a)-(b)c")]
		[InlineData ("abc", "", "-(a)-(b)-(c)")]
		public void SimpleCases (string left, string right, string expectedDiff)
		{
			var diff = left.Diff (right);
			Assert.Equal (expectedDiff, diff.ToString ());
		}

		[Theory]
		[InlineData (1000)]
		[InlineData (10000)]
		[InlineData (100000)]
		public void DiffMiddleModificationOfLongList (int listSize)
		{
			var sb = new System.Text.StringBuilder ();
			for (int i = 0; i < listSize / 10; i++) {
				sb.Append ("0123456789");
			}

			var original = sb.ToString ();
			var middleIndex = original.Length / 2;
			var middleItem = original[middleIndex];
			var modified = sb.ToString ().Remove (middleIndex, 1);
			var expectedDiff = modified.Insert (middleIndex, string.Format("-({0})", middleItem));

			var diff = original.Diff (modified);

			Assert.Equal (expectedDiff, diff.ToString ());
		}

		[Fact]
		public void Insert ()
		{
			var source = "ac";
			var destination = "abc";
			var diff = source.Diff (destination);

			Assert.Equal (3, diff.Actions.Count);
			Assert.Equal (ListDiffActionType.Add, diff.Actions[1].ActionType);
		}

		[Theory]
		[InlineData ("", "")]
		[InlineData ("", "abc")]
		[InlineData ("abc", "")]
		[InlineData ("abc", "abc")]
		[InlineData ("abc", "abcd")]
		[InlineData ("abc", "ab")]
		[InlineData ("abc", "dabc")]
		[InlineData ("abc", "bc")]
		[InlineData ("abcde34fg", "bXceYZf348n")]
		public void RemovesAndAdds (string source, string dest)
		{
			var (rems, adds) = source.Diff (dest).GetRemovesAndAdds ();

			//
			// Assert grouping on trivial cases
			//
			if (source.Length > 0 && dest.Length == 0) {
				Assert.Single (rems);
				Assert.Equal (0, rems[0].Index);
				Assert.Equal (source.Length, rems[0].Count);
			}
			if (source.Length == 0 && dest.Length > 0) {
				Assert.Single (adds);
				Assert.Equal (0, adds[0].Index);
				Assert.Equal (dest.Length, adds[0].Items.Length);
			}
			if (source == dest) {
				Assert.Empty (adds);
				Assert.Empty (rems);
			}

			//
			// Test the operations
			//
			var msource = new List<char> (source);
			foreach (var r in rems) {
				for (var i = 0; i < r.Count; i++) {
					msource.RemoveAt (r.Index);
				}
			}
			foreach (var a in adds) {
				var i = a.Index;
				foreach (var item in a.Items) {
					msource.Insert (i, item);
					i++;
				}
			}
			var ssource = new string (msource.ToArray ());
			Assert.Equal (dest, ssource);
		}
	}
}
