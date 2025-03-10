using ToSic.Eav.DataSourceTests;

namespace ToSic.Eav.DataSources.Various;
// Todo
// Create tests with language-parameters as well, as these tests ignore the language and always use default

[Startup(typeof(StartupTestsEavCoreAndDataSources))]
public class ShuffleTest(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    //private const int TestVolume = 10000;
    //private ValueFilter _testDataGeneratedOutsideTimer;
    //public ValueFilterBoolean()
    //{
    //    //_testDataGeneratedOutsideTimer = ValueFilter_Test.CreateValueFilterForTesting(TestVolume);
    //}


    #region shuffle tests


    private Shuffle GenerateShuffleDS(int desiredFinds)
    {
        var ds = personTableGenerator.New().Generate(desiredFinds, 1001, true);
        var sf = DsSvc.CreateDataSource<Shuffle>(ds);
        return sf;
    }


    [Fact]
    public void Shuffle_CountShuffle100()
    {
        var desiredFinds = 100;
        var sf = GenerateShuffleDS(desiredFinds);
        var found = sf.ListTac().Count();
        Equal(desiredFinds, found);//, "Should find exactly this amount people");

    }

    [Fact]
    public void Shuffle5_ValidateNotOrdered()
    {
        var items = 5;
        var sf = GenerateShuffleDS(items);

        var origSeqSorted = AreAllItemsSorted(sf.InTac()[DataSourceConstants.StreamDefaultName]);
        var seqConsistent = AreAllItemsSorted(sf.Out[DataSourceConstants.StreamDefaultName]);

        // after checking all, it should NOT be consistent
        True(origSeqSorted, "original sequence SHOULD be sorted");
        False(seqConsistent, "sequence should NOT be not-sorted");

    }

    private static bool AreAllItemsSorted(IDataStream sf)
    {
        // now the IDs shouldn't be incrementing one after another
        var seqConsistent = true;
        var lastId = 0;
        foreach (var itm in sf.ListTac())
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