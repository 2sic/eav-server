using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests.Streams;

[TestClass]
public class StreamMergeTst: TestBaseEavDataSource
{
    private const int ItemsToGenerate = 100;
    private const int FirstId = 1001;

    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void StreamMerge_In0Streams()
    {
        var sf = DsSvc.CreateDataSource<StreamMerge>();
        VerifyStreams(sf, 0, 0, 0, 0);
    }

    [TestMethod]
    public void StreamMerge_In1Stream()
    {
        var streamMerge = CreateMergeDs(ItemsToGenerate);
        VerifyStreams(streamMerge, ItemsToGenerate, ItemsToGenerate, ItemsToGenerate, ItemsToGenerate);
    }

    [TestMethod]
    public void StreamMerge_In2IdenticalStreams()
    {
        var desiredFinds = ItemsToGenerate * 2;
        var sf = CreateMergeDs(ItemsToGenerate);
        sf.Attach("another", sf.InTac().FirstOrDefault().Value);
        VerifyStreams(sf, desiredFinds, ItemsToGenerate, ItemsToGenerate, 0);
    }


    [DataRow(2, "two lists")]
    [DataRow(3, "three lists")]
    [DataRow(4, "four lists")]
    [TestMethod]
    public void StreamMerge_InDifferentMany(int amount, string name)
    {
        var desiredFinds = ItemsToGenerate * amount;
        var main = CreateMergeDs(ItemsToGenerate);

        // Additional has the same amount, but they should be different entity objects
        for (var i = 1; i < amount; i++)
            main.Attach("another" + i, SourceList(ItemsToGenerate));

        VerifyStreams(main, desiredFinds, desiredFinds, 0, desiredFinds);
    }

    [DataRow(0.5, "half")]
    [DataRow(0.25, "quarter")]
    [TestMethod]
    public void StreamMerge_In2WithHalf(double fraction, string name)
    {
        var itemsInSecondStream = (int)(ItemsToGenerate * fraction);
        var desiredFinds = ItemsToGenerate + itemsInSecondStream;
        var sf = CreateMergeDs(ItemsToGenerate);

        // Second has the same amount, but they should be different entity objects
        var secondSf = GenerateSecondStreamWithSomeResults(sf, itemsInSecondStream);
        sf.Attach("another", secondSf.StreamForTests());
        VerifyStreams(sf, desiredFinds, ItemsToGenerate, itemsInSecondStream, ItemsToGenerate - itemsInSecondStream);
    }


    [TestMethod]
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

    private ValueFilter GenerateSecondStreamWithSomeResults(StreamMerge sf, int itemsInSecondStream)
    {
        var secondSf = DsSvc.CreateDataSource<ValueFilter>(sf.InTac().First().Value);
        secondSf.Attribute = Attributes.EntityIdPascalCase;
        secondSf.Operator = "<";
        secondSf.Value = (FirstId + itemsInSecondStream).ToString();
        Assert.AreEqual(itemsInSecondStream, secondSf.ListForTests().Count(),
            $"For next test to work, we must be sure we have {itemsInSecondStream} items here");
        return secondSf;
    }



    private StreamMerge CreateMergeDs(int desiredFinds) =>
        DsSvc.CreateDataSource<StreamMerge>(SourceList(desiredFinds));

    private DataTable SourceList(int desiredFinds, int firstId = FirstId) =>
        new DataTablePerson(this).Generate(desiredFinds, firstId, useCacheForSpeed: false /* don't cache, because we do duplicate checking in these tests */);
}