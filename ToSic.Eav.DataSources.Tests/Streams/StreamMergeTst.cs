using System.Diagnostics;
using ToSic.Eav.Data.Sys;

#pragma warning disable xUnit1026

namespace ToSic.Eav.DataSourceTests.Streams;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class StreamMergeTst(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    private const int ItemsToGenerate = 100;
    private const int FirstId = 1001;


    [Fact]
    public void StreamMerge_In0Streams()
    {
        var sf = DsSvc.CreateDataSource<StreamMerge>();
        VerifyStreams(sf, 0, 0, 0, 0);
    }

    [Fact]
    public void StreamMerge_In1Stream()
    {
        var streamMerge = CreateMergeDs(ItemsToGenerate);
        VerifyStreams(streamMerge, ItemsToGenerate, ItemsToGenerate, ItemsToGenerate, ItemsToGenerate);
    }

    [Fact]
    public void StreamMerge_In2IdenticalStreams()
    {
        var desiredFinds = ItemsToGenerate * 2;
        var sf = CreateMergeDs(ItemsToGenerate);
        sf.Attach("another", sf.InTac().FirstOrDefault().Value);
        VerifyStreams(sf, desiredFinds, ItemsToGenerate, ItemsToGenerate, 0);
    }


    [Theory]
    [InlineData(2, "two lists")]
    [InlineData(3, "three lists")]
    [InlineData(4, "four lists")]
    public void StreamMerge_InDifferentMany(int amount, string name)
    {
        var desiredFinds = ItemsToGenerate * amount;
        var main = CreateMergeDs(ItemsToGenerate);

        // Additional has the same amount, but they should be different entity objects
        for (var i = 1; i < amount; i++)
            main.Attach("another" + i, SourceList(ItemsToGenerate));

        VerifyStreams(main, desiredFinds, desiredFinds, 0, desiredFinds);
    }

    [Theory]
    [InlineData(0.5, "half")]
    [InlineData(0.25, "quarter")]
    public void StreamMerge_In2WithHalf(double fraction, string name)
    {
        var itemsInSecondStream = (int)(ItemsToGenerate * fraction);
        var desiredFinds = ItemsToGenerate + itemsInSecondStream;
        var sf = CreateMergeDs(ItemsToGenerate);

        // Second has the same amount, but they should be different entity objects
        var secondSf = GenerateSecondStreamWithSomeResults(sf, itemsInSecondStream);
        sf.Attach("another", secondSf.GetStreamTac());
        VerifyStreams(sf, desiredFinds, ItemsToGenerate, itemsInSecondStream, ItemsToGenerate - itemsInSecondStream);
    }


    [Fact]
    public void StreamMerge_In2Null1()
    {
        var desiredFinds = ItemsToGenerate * 3;
        var sf = CreateMergeDs(ItemsToGenerate);
        sf.Attach("another", sf.InTac().FirstOrDefault().Value);
        sf.Attach("middle", null);
        sf.Attach("xFinal", sf.InTac().FirstOrDefault().Value);

        VerifyStreams(sf, desiredFinds, ItemsToGenerate, ItemsToGenerate, 0);
    }

    private void VerifyStreams(StreamMerge streamMerge, int expDefault, int expDistinct, int expAnd, int expXor)
    {
        var found = streamMerge.ListTac().Count();
        Trace.WriteLine("Found (Default / OR): " + found);
        Equal(expDefault, found); //, "Should find exactly this amount people after merge");

        var foundDistinct = streamMerge.OutTac(StreamMerge.DistinctStream).Count();
        Trace.WriteLine("Distinct: " + foundDistinct);
        Equal(expDistinct, foundDistinct); //, "Should find exactly this amount of _distinct_ people");

        var foundAnd = streamMerge.OutTac(StreamMerge.AndStream).Count();
        Trace.WriteLine("AND: " + foundAnd);
        Equal(expAnd, foundAnd); //, "Should find exactly this amount of _AND_ people");

        var foundXor = streamMerge.OutTac(StreamMerge.XorStream).Count();
        Trace.WriteLine("XOR: " + foundXor);
        Equal(expXor, foundXor); //, "Should find exactly this amount of _XOR_ people");
    }

    private ValueFilter GenerateSecondStreamWithSomeResults(StreamMerge sf, int itemsInSecondStream)
    {
        var secondSf = DsSvc.CreateDataSource<ValueFilter>(sf.InTac().First().Value);
        secondSf.Attribute = AttributeNames.EntityIdPascalCase;
        secondSf.Operator = "<";
        secondSf.Value = (FirstId + itemsInSecondStream).ToString();
        Equal(itemsInSecondStream, secondSf.ListTac().Count()); //, $"For next test to work, we must be sure we have {itemsInSecondStream} items here");
        return secondSf;
    }



    private StreamMerge CreateMergeDs(int desiredFinds) =>
        DsSvc.CreateDataSource<StreamMerge>(SourceList(desiredFinds));

    private DataTable SourceList(int desiredFinds, int firstId = FirstId) =>
        personTableGenerator.New().Generate(desiredFinds, firstId, useCacheForSpeed: false /* don't cache, because we do duplicate checking in these tests */);
}