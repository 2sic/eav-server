using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps.Tests
{
    [TestClass]
    public partial class ListPairTests
    {
        public const string PName = "Primary";
        public const string CName = "Coupled";

        [TestMethod]
        public void UnmatchedPairsAutoFixShort()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {null, null},
                PName,
                CName, null);
            AssertLength(pair, 4);
        }

        [TestMethod]
        public void UnmatchedPairsAutoFixLong()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {null, null, null, null, null, null},
                PName,
                CName, null);
            AssertLength(pair, 4);
        }

        private void AssertLength(ListPair pair, int expectedLength)
        {
            var itemsNotWithLength = pair.Lists.Values.Where(list => list.Count != expectedLength);
            Assert.AreEqual(0, itemsNotWithLength.Count(), "lengths should now match");
            //Assert.AreEqual(expectedLength, pair.CoupledIds.Count, $"length was expected to be {expectedLength}");
        }

        private void AssertPositions(ListPair pair, int position, int? primary, int? coupled)
        {
            var found = pair.Lists.Values.First()[position];
            Assert.AreEqual(primary, found, 
                $"Primary at {position} expected to be {primary} but was {found}");
            found = pair.Lists.Values.Skip(1).First()[position];
            Assert.AreEqual(coupled, found, 
                $"Primary at {position} expected to be {primary} but was {found}");
        }

        private void AssertSequence(ListPair pair, int startIndex)
        {

        }

    }
}
