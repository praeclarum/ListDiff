using System;
using System.Collections.Generic;
using ListDiff;
using Xunit;

namespace ListDiffTests
{
	public class Tests {
		[Fact]
		public void Insert () {
			var source = "ac";
			var destination = "abc";
			var diff = source.Diff (destination);
		}
	}
}
