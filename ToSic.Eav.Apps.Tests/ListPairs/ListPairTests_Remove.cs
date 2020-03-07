﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.Parts;

namespace ToSic.Eav.Apps.Tests
{
    public partial class ListPairTests
    {

        [TestMethod]
        public void RemoveAt0()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {101, null, 103, null, null, null},
                PName,
                CName, null);
            pair.Remove(0);
            AssertLength(pair, 3);
            Assert.AreEqual(pair.PrimaryIds.First(), 2);
            Assert.AreEqual(pair.CoupledIds.First(), null);
        }

        [TestMethod]
        public void RemoveAt1()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {101, null, 103, null, null, null},
                PName,
                CName, null);
            pair.Remove(1);
            AssertLength(pair, 3);
            Assert.AreEqual(pair.PrimaryIds.First(), 1);
            Assert.AreEqual(pair.CoupledIds.First(), 101);
            Assert.AreEqual(pair.PrimaryIds[1], null);
            Assert.AreEqual(pair.CoupledIds[1], 103);
        }

        [TestMethod]
        public void RemoveAtEnd()
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {101, null, 103, 104, null, null},
                PName,
                CName, null);
            pair.Remove(3);
            AssertLength(pair, 3);
            Assert.AreEqual(pair.PrimaryIds.First(), 1);
            Assert.AreEqual(pair.CoupledIds.First(), 101);
            Assert.AreEqual(pair.PrimaryIds.Last(), null);
            Assert.AreEqual(pair.CoupledIds.Last(), 103);
        }


        [TestMethod]
        public void RemoveAfterEnd() => RemoveAtEndOrBeyond(4);

        [TestMethod]
        public void RemoveAfterEndFarBehind() => RemoveAtEndOrBeyond(9);

        private void RemoveAtEndOrBeyond(int addPosition)
        {
            var pair = new ListPair(new List<int?> {1, 2, null, 3},
                new List<int?> {101, null, 103, null, null, null},
                PName,
                CName, null);
            pair.Remove(addPosition);
            AssertLength(pair, 4);
            Assert.AreEqual(pair.PrimaryIds.First(), 1);
            Assert.AreEqual(pair.CoupledIds.First(), 101);
            Assert.AreEqual(pair.PrimaryIds[1], 2);
            Assert.AreEqual(pair.CoupledIds[1], null);
            Assert.AreEqual(pair.PrimaryIds[3], 3);
            Assert.AreEqual(pair.CoupledIds[3], null);
        }

    }
}
