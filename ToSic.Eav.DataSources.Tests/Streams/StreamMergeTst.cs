using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Streams
{
    [TestClass]
    public class StreamMergeTst: TestBaseEavDataSource
    {
        const int ItemsToGenerate = 100;
        private const int FirstId = 1001;

        [TestMethod]
        public void StreamMerge_In0()
        {
            var sf = CreateDataSource<StreamMerge>();
            VerifyStreams(sf, 0, 0, 0, 0);
        }

        [TestMethod]
        public void StreamMerge_In1()
        {
            var desiredFinds = 100;
            var streamMerge = GenerateMergeDs(ItemsToGenerate);
            VerifyStreams(streamMerge, desiredFinds, ItemsToGenerate, ItemsToGenerate, desiredFinds);
        }

        private void VerifyStreams(StreamMerge streamMerge, int expDefault, int expDistinct, int expAnd, int expXor)
        {
            var found = streamMerge.ListForTests().Count();
            Trace.WriteLine("Found (Default / OR): " + found);
            Assert.AreEqual(expDefault, found, "Should find exactly this amount people after merge");

            var foundDistinct = streamMerge.ListForTests(StreamMerge.DistinctStream).Count();
            Trace.WriteLine("Distinct: " + foundDistinct);
            Assert.AreEqual(expDistinct, foundDistinct, "Should find exactly this amount of _distinct_ people");

            var foundAnd = streamMerge.ListForTests(StreamMerge.AndStream).Count();
            Trace.WriteLine("AND: " + foundAnd);
            Assert.AreEqual(expAnd, foundAnd, "Should find exactly this amount of _AND_ people");

            var foundXor = streamMerge.ListForTests(StreamMerge.XorStream).Count();
            Trace.WriteLine("XOR: " + foundXor);
            Assert.AreEqual(expXor, foundXor, "Should find exactly this amount of _XOR_ people");
        }

        [TestMethod]
        public void StreamMerge_In2Same()
        {
            var desiredFinds = ItemsToGenerate * 2;
            var sf = GenerateMergeDs(ItemsToGenerate);
            sf.Attach("another", sf.InForTests().FirstOrDefault().Value);
            VerifyStreams(sf, desiredFinds, ItemsToGenerate, ItemsToGenerate, 0);
        }

        [TestMethod]
        public void StreamMerge_In2Different()
        {
            var desiredFinds = ItemsToGenerate * 2;
            var sf = GenerateMergeDs(ItemsToGenerate);

            // Second has the same amount, but they should be different entity objects
            var secondSf = GenerateMergeDs(ItemsToGenerate);

            sf.Attach("another", secondSf.InForTests().First().Value);
            VerifyStreams(sf, desiredFinds, desiredFinds, 0, desiredFinds);
        }

        [TestMethod]
        public void StreamMerge_In2WithHalf()
        {
            var itemsInSecondStream = 50;
            var desiredFinds = ItemsToGenerate + itemsInSecondStream;
            var sf = GenerateMergeDs(ItemsToGenerate);

            // Second has the same amount, but they should be different entity objects
            var secondSf = GenerateSecondStreamWithSomeResults(sf, itemsInSecondStream);
            sf.Attach("another", secondSf.StreamForTests());
            VerifyStreams(sf, desiredFinds, ItemsToGenerate, itemsInSecondStream, ItemsToGenerate - itemsInSecondStream);
        }

        [TestMethod]
        public void StreamMerge_In2WithQuarter()
        {
            var itemsInSecondStream = 25;
            var desiredFinds = ItemsToGenerate + itemsInSecondStream;
            var sf = GenerateMergeDs(ItemsToGenerate);

            // Second has the same amount, but they should be different entity objects
            var secondSf = GenerateSecondStreamWithSomeResults(sf, itemsInSecondStream);
            sf.Attach("another", secondSf.StreamForTests());
            VerifyStreams(sf, desiredFinds, ItemsToGenerate, itemsInSecondStream, ItemsToGenerate - itemsInSecondStream);
        }


        private ValueFilter GenerateSecondStreamWithSomeResults(StreamMerge sf, int itemsInSecondStream)
        {
            var secondSf = CreateDataSource<ValueFilter>(sf.InForTests().First().Value);
            secondSf.Attribute = Attributes.EntityIdPascalCase;
            secondSf.Operator = "<";
            secondSf.Value = (FirstId + itemsInSecondStream).ToString();
            Assert.AreEqual(itemsInSecondStream, secondSf.ListForTests().Count(),
                $"For next test to work, we must be sure we have {itemsInSecondStream} items here");
            return secondSf;
        }

        [TestMethod]
        public void StreamMerge_In2Null1()
        {
            var desiredFinds = ItemsToGenerate * 3;
            var sf = GenerateMergeDs(ItemsToGenerate);
            sf.Attach("another", sf.InForTests().FirstOrDefault().Value);
            sf.Attach("middle", null);
            sf.Attach("xFinal", sf.InForTests().FirstOrDefault().Value);

            VerifyStreams(sf, desiredFinds, ItemsToGenerate, ItemsToGenerate, 0);
        }

        private StreamMerge GenerateMergeDs(int desiredFinds)
        {
            var ds = new DataTablePerson(this).Generate(desiredFinds, FirstId, 
                false // important: don't cache, because we do duplicate checking in these tests
                );
            var sf = CreateDataSource<StreamMerge>(ds);
            return sf;
        }
    }
}
