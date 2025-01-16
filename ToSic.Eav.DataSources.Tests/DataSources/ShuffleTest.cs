using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[TestClass]
public class ShuffleTest: TestBaseEavDataSource
{
    //private const int TestVolume = 10000;
    //private ValueFilter _testDataGeneratedOutsideTimer;
    //public ValueFilterBoolean()
    //{
    //    //_testDataGeneratedOutsideTimer = ValueFilter_Test.CreateValueFilterForTesting(TestVolume);
    //}
        

    #region shuffle tests


    private DataSources.Shuffle GenerateShuffleDS(int desiredFinds)
    {
        var ds = new DataTablePerson(this).Generate(desiredFinds, 1001, true);
        var sf = CreateDataSource<Shuffle>(ds);// DataSourceFactory.GetDataSource<Shuffle>(new AppIdentity(0, 0), ds);
        return sf;
    }


    [TestMethod]
    public void Shuffle_CountShuffle100()
    {
        var desiredFinds = 100;
        var sf = GenerateShuffleDS(desiredFinds);
        var found = sf.ListForTests().Count();
        Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");

    }

    [TestMethod]
    public void Shuffle5_ValidateNotOrdered()
    {
        var items = 5;
        var sf = GenerateShuffleDS(items);

        var origSeqSorted = AreAllItemsSorted(sf.InForTests()[DataSourceConstants.StreamDefaultName]);
        var seqConsistent = AreAllItemsSorted(sf.Out[DataSourceConstants.StreamDefaultName]);

        // after checking all, it should NOT be consistent
        Assert.IsTrue(origSeqSorted, "original sequence SHOULD be sorted");
        Assert.IsFalse(seqConsistent, "sequence should NOT be not-sorted");

    }

    private static bool AreAllItemsSorted(IDataStream sf)
    {
        // now the IDs shouldn't be incrementing one after another
        var seqConsistent = true;
        var lastId = 0;
        foreach (var itm in sf.ListForTests())
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