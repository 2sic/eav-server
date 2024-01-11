using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Internal.Work;

namespace ToSic.Eav.Apps.Tests
{
    public partial class ListPairTests
    {

        [TestMethod]
        public void ReorderUnchanged()
        {
            var pair = GenerateListAndReorder(new[] { 0, 1, 2, 3 });
            AssertLength(pair, 4);
            AssertPositions(pair,0, 1, 101);
            AssertPositions(pair, 1, 2, null);
            AssertPositions(pair,2, null, 103);
            AssertPositions(pair,3, 44, 444);
        }

        [TestMethod]
        public void Reorder1023()
        {
            var pair = GenerateListAndReorder(new[] { 1, 0, 2, 3 });
            AssertLength(pair, 4);
            AssertPositions(pair, 0, 2, null);
            AssertPositions(pair, 1, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 44, 444);
        }

        [TestMethod]
        public void Reorder3201()
        {
            var pair = GenerateListAndReorder(new[] {3, 2, 0, 1});
            AssertLength(pair, 4);
            AssertPositions(pair, 0, 44, 444);
            AssertPositions(pair, 1, null, 103);
            AssertPositions(pair, 2, 1, 101);
            AssertPositions(pair, 3, 2, null);
        }

        /// <summary>
        /// This test has too many re-order items but valid indexes
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ReorderErrorLongerSequence() 
            => GenerateListAndReorder(new[] {3, 2, 0, 1, 0, 1});

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ReorderErrorShorterSequence() 
            => GenerateListAndReorder(new[] {3, 2, 0});

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ReorderErrorReUse() 
            => GenerateListAndReorder(new[] {3, 2, 3, 0});

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReorderErrorOutOfRange() 
            => GenerateListAndReorder(new[] {3, 7, 0, 1});

        private static CoupledIdLists GenerateListAndReorder(int[] newSequence)
        {
            var pair = CoupledIdLists(new List<int?> {1, 2, null, 44},
                new List<int?> {101, null, 103, 444},
                PName, CName);
            pair.Reorder(newSequence);
            return pair;
        }
    }
}
