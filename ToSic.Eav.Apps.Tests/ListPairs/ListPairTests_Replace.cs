using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Parts.Tools;

namespace ToSic.Eav.Apps.Tests
{
    public partial class ListPairTests
    {

        [TestMethod]
        public void ReplaceAt0Primary()
        {
            var pair = ReplaceAtX(0, false);
            AssertLength(pair, 4);
            AssertPositions(pair,0, 999, 101);
            AssertPositions(pair,1, 2, null);
        }

        [TestMethod]
        public void ReplaceAt0Both()
        {
            var pair = ReplaceAtX(0, true);
            AssertLength(pair, 4);
            AssertPositions(pair,0, 999, 777);
            AssertPositions(pair,1, 2, null);
        }

        [TestMethod]
        public void ReplaceAt1Primary()
        {
            var pair = ReplaceAtX(1, false);
            AssertLength(pair, 4);
            AssertPositions(pair,0, 1, 101);
            AssertPositions(pair,1, 999, null);
            AssertPositions(pair,2, null, 103);
        }

        [TestMethod]
        public void ReplaceAt1Both()
        {
            var pair = ReplaceAtX(1, true);
            AssertLength(pair, 4);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 1, 999, 777);
            AssertPositions(pair, 2, null, 103);
        }

        [TestMethod]
        public void ReplaceAtEndPrimary()
        {
            var pair = ReplaceAtX(3, false);
            AssertLength(pair, 4);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 999, null);
        }

        [TestMethod]
        public void ReplaceAtEndBoth()
        {
            var pair = ReplaceAtX(3, true);
            AssertLength(pair, 4);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 999, 777);
        }

        [TestMethod]
        public void ReplaceAfterEndPrimary()
        {
            var pair = ReplaceAtX(4, false);
            AssertLength(pair, 4 + 1);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 44, null);
            AssertPositions(pair, 4, 999, null);
        }

        [TestMethod]
        public void ReplaceAfterEndBoth()
        {
            var pair = ReplaceAtX(4, true);
            AssertLength(pair, 4 + 1);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 44, null);
            AssertPositions(pair, 4, 999, 777);
        }

        [TestMethod]
        public void ReplacePastEndPrimary()
        {
            var pair = ReplaceAtX(9, false);
            AssertLength(pair, 10);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 44, null);
            AssertPositions(pair, 4, null, null);
            AssertPositions(pair, 9, 999, null);
        }

        [TestMethod]
        public void ReplacePastEndBoth()
        {
            var pair = ReplaceAtX(9, true);
            AssertLength(pair, 10);
            AssertPositions(pair, 0, 1, 101);
            AssertPositions(pair, 2, null, 103);
            AssertPositions(pair, 3, 44, null);
            AssertPositions(pair, 4, null, null);
            AssertPositions(pair, 9, 999, 777);
        }


        private static CoupledIdLists ReplaceAtX(int index, bool updatePair)
        {
            var pair = CoupledIdLists(new List<int?> { 1, 2, null, 44 },
                new List<int?> { 101, null, 103, null, null, null },
                PName, CName);
            pair.Replace(index, new[]
            {
                new Tuple<bool, int?>(true, 999), 
                new Tuple<bool, int?>(updatePair, 777), 
            });
            return pair;
        }


    }
}
