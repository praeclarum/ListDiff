//
// Copyright (c) Krueger Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace ListDiff
{
	static partial class DiffModule
	{
		public static List<(A, T, T)> Diff<T, A> (IEnumerable<T> source, IEnumerable<T> destination,
		                                          A update, A add, A remove) =>
			Diff (source, destination, update, add, remove, out _);

		public static List<(A, T, T)> Diff<T, A> (IEnumerable<T> source, IEnumerable<T> destination,
												  A update, A add, A remove,
												  out bool containsOnlyUpdates) =>
			Diff (source, destination, (s, d) => EqualityComparer<T>.Default.Equals (s, d),
			      update, add, remove, out containsOnlyUpdates);

		public static List<(A, S, D)> Diff<S, D, A> (IEnumerable<S> source, IEnumerable<D> destination,
		                                             Func<S, D, bool> match,
		                                             A update, A add, A remove) =>
			Diff (source, destination, match, update, add, remove, out _);

		public static List<(A, S, D)> Diff<S, D, A> (IEnumerable<S> source, IEnumerable<D> destination,
		                                             Func<S, D, bool> match,
		                                             A update, A add, A remove,
		                                             out bool containsOnlyUpdates) =>
			Diff (source, destination, match, (s, d) => (update, s, d),
											  d => (add, default, d),
											  s => (remove, s, default),
											  out containsOnlyUpdates);

		public static List<R> Diff<T, R> (IEnumerable<T> source, IEnumerable<T> destination,
		                                  Func<T, T, R> updateResult, Func<T, R> addResult, Func<T, R> removeResult) =>
			Diff (source, destination, updateResult, addResult, removeResult, out _);

		public static List<R> Diff<T, R> (IEnumerable<T> source, IEnumerable<T> destination,
		                                  Func<T, T, R> updateResult, Func<T, R> addResult, Func<T, R> removeResult,
		                                  out bool containsOnlyUpdates) =>
			Diff (source, destination, (s, d) => EqualityComparer<T>.Default.Equals (s, d),
			      updateResult, addResult, removeResult, out containsOnlyUpdates);

		public static List<R> Diff<S, D, R> (IEnumerable<S> source, IEnumerable<D> destination,
											 Func<S, D, bool> match,
											 Func<S, D, R> updateResult, Func<D, R> addResult, Func<S, R> removeResult) =>
			Diff (source, destination, match, updateResult, addResult, removeResult, out _);

		public static List<R> Diff<S, D, R> (IEnumerable<S> source, IEnumerable<D> destination,
		                                     Func<S, D, bool> match,
											 Func<S, D, R> updateResult, Func<D, R> addResult, Func<S, R> removeResult,
											 out bool containsOnlyUpdates)
		{
			if (source == null) throw new ArgumentNullException (nameof (source));
			if (destination == null) throw new ArgumentNullException (nameof (destination));
			if (match == null) throw new ArgumentNullException (nameof (match));

			IList<S> x = source as IList<S> ?? source.ToArray ();
			IList<D> y = destination as IList<D> ?? destination.ToArray ();

			var actions = new List<R> ();

			var m = x.Count;
			var n = y.Count;

			var start = 0;

			while (start < m && start < n && match (x[start], y[start])) {
				start++;
			}

			while (start < m && start < n && match (x[m - 1], y[n - 1])) {
				m--;
				n--;
			}

			//
			// Construct the C matrix
			//
			var c = new int[m - start + 1, n - start + 1];
			for (var i = 1; i <= m - start; i++) {
				for (var j = 1; j <= n - start; j++) {
					if (match (x[i - 1], y[j - 1])) {
						c[i, j] = c[i - 1, j - 1] + 1;
					}
					else {
						c[i, j] = Math.Max (c[i, j - 1], c[i - 1, j]);
					}
				}
			}

			//
			// Generate the actions
			//
			for (int i = 0; i < start; i++) {
				actions.Add (updateResult (x[i], y[i]));
			}

			var varContainsOnlyUpdates = true;
			GenDiff (m, n);

			for (int i = 0; i < x.Count - m; i++) {
				actions.Add (updateResult (x[m + i], y[n + i]));
			}

			containsOnlyUpdates = varContainsOnlyUpdates;
			return actions;

			void GenDiff (int i, int j)
			{
				if (i > start && j > start && match (x[i - 1], y[j - 1])) {
					GenDiff (i - 1, j - 1);
					actions.Add (updateResult (x[i - 1], y[j - 1]));
				}
				else {
					if (j > start && (i == start || c[i - start, j - start - 1] >= c[i - start - 1, j - start])) {
						GenDiff (i, j - 1);
						varContainsOnlyUpdates = false;
						actions.Add (addResult (y[j - 1]));
					}
					else if (i > start && (j == start || c[i - start, j - start - 1] < c[i - start - 1, j - start])) {
						GenDiff (i - 1, j);
						varContainsOnlyUpdates = false;
						actions.Add (removeResult (x[i - 1]));
					}
				}
			}
		}
	}
}
