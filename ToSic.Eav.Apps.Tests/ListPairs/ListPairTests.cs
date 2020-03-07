using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps.Tests
{
    [TestClass]
    public class ListPairTests
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
            Assert.AreEqual(pair.PrimaryIds.Count, pair.CoupledIds.Count, "lengths should now match");
        }

        [TestMethod]
        public void UnmatchedPairsAutoFixLong()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {null, null, null, null, null, null},
                PName,
                CName, null);
            Assert.AreEqual(pair.PrimaryIds.Count, pair.CoupledIds.Count, "lengths should now match");
        }

    }
}
