﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class Shuffle: EavTestBase
    {
        //private const int TestVolume = 10000;
        //private ValueFilter _testDataGeneratedOutsideTimer;
        //public ValueFilterBoolean()
        //{
        //    //_testDataGeneratedOutsideTimer = ValueFilter_Test.CreateValueFilterForTesting(TestVolume);
        //}
        

        #region shuffle tests


        private static DataSources.Shuffle GenerateShuffleDS(int desiredFinds)
        {
            var ds = DataTablePerson.Generate(desiredFinds, 1001, true);
            var sf = Resolve<DataSourceFactory>().GetDataSource<DataSources.Shuffle>(new AppIdentity(0, 0), ds);
            return sf;
        }


        [TestMethod]
        public void Shuffle_CountShuffle100()
        {
            var desiredFinds = 100;
            var sf = GenerateShuffleDS(desiredFinds);
            var found = sf.List.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

        }

        [TestMethod]
        public void Shuffle5_ValidateNotOrdered()
        {
            var items = 5;
            var sf = GenerateShuffleDS(items);

            var origSeqSorted = AreAllItemsSorted(sf.In[Constants.DefaultStreamName]);
            var seqConsistent = AreAllItemsSorted(sf.Out[Constants.DefaultStreamName]);

            // after checking all, it should NOT be consistent
            Assert.IsTrue(origSeqSorted, "original sequence SHOULD be sorted");
            Assert.IsFalse(seqConsistent, "sequence should NOT be not-sorted");

        }

        private static bool AreAllItemsSorted(IDataStream sf)
        {
            // now the IDs shouldn't be incrementing one after another
            var seqConsistent = true;
            var lastId = 0;
            foreach (var itm in sf.Immutable)
            {
                var newId = itm.EntityId;
                if (newId < lastId)
                    seqConsistent = false;
                lastId = newId;
            }
            return seqConsistent;
        }

        #endregion

    }
}
