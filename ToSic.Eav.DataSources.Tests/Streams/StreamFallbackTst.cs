using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;

namespace ToSic.Eav.DataSourceTests.Streams;

[TestClass]
public class StreamFallbackTst: TestBaseEavDataSource
{
    [TestMethod]
    public void StreamWhereDefaultIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        stmf.Attach(StreamDefaultName, stmf.InForTests()["1"]);

        Assert.AreEqual(1, stmf.ListForTests().Count(), "should have found 1");
    }

    [TestMethod]
    public void StreamsWhereFirstFallbackIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        Assert.AreEqual(1, stmf.ListForTests().Count(), "should be 1 - the fallback");
    }

    [TestMethod]
    public void DoManyFoldFallback()
    {
        var stmf = AssembleTestFallbackStream(addStream1: false);
        //stmf.InForTests().Remove("1");
        var inDefaultStream = stmf.InForTests()[StreamDefaultName];
        stmf.Attach("Fallback1", inDefaultStream);
        stmf.Attach("Fallback2", inDefaultStream);
        stmf.Attach("Fallback3", inDefaultStream);
        stmf.Attach("Fallback4", inDefaultStream);
        stmf.Attach("Fallback5", inDefaultStream);

        Assert.AreEqual(45, stmf.ListForTests().Count(), "Should have looped through many and found the 45");
        Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
    }

    [TestMethod]
    public void DoManyFallbacks2()
    {
        var sourceForStream1 = AssembleTestFallbackStream(addStream1: true);
        var stmf = AssembleTestFallbackStream(addStream1: false);
        stmf.Attach("ZZZ", sourceForStream1.InForTests()["1"]); // should be after the "ZMany" so it should not return anything
        stmf.Attach("1", stmf.InForTests()[StreamDefaultName]);
        stmf.Attach("2", stmf.InForTests()[StreamDefaultName]);
        stmf.Attach("3", stmf.InForTests()[StreamDefaultName]);
        stmf.Attach("Fallback5", stmf.InForTests()[StreamDefaultName]);
        Assert.AreEqual(45, stmf.ListForTests().Count(), "Should have looped through many and found the 45");
        Assert.AreEqual("ZMany", stmf.ReturnedStreamName);
    }

    [TestMethod]
    public void FindNothing()
    {
        var stmf = AssembleTestFallbackStream(addStream1: false, addStreamZMany: false);
        stmf.Attach("1", stmf.InForTests()[StreamDefaultName]);
        stmf.Attach("2", stmf.InForTests()[StreamDefaultName]);
        stmf.Attach("3", stmf.InForTests()[StreamDefaultName]);
        Assert.AreEqual(0, stmf.ListForTests().Count(), "Should find none");
    }

    private StreamFallback AssembleTestFallbackStream(bool addStream1 = true, bool addStreamZMany = true)
    {
        var emptyDs = new DataTablePerson(this).Generate(0, 1001);
        var streams = CreateDataSource<StreamFallback>(emptyDs);

        var dsWith1 = new DataTablePerson(this).Generate(1, 2000);
        var dsWithmany = new DataTablePerson(this).Generate(45, 4000);
        if (addStream1)
            streams.AttachForTests("1", dsWith1);
        if (addStreamZMany)
            streams.AttachForTests("ZMany", dsWithmany);
        return streams;

    }
}