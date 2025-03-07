using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSourceTests.Streams;

[TestClass]
public class StreamFallbackTst: TestBaseEavDataSource
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    [TestMethod]
    public void StreamWhereDefaultIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        stmf.Attach(StreamDefaultName, stmf.InTac()["1"]);

        AreEqual(1, stmf.ListTac().Count(), "should have found 1");
    }

    [TestMethod]
    public void StreamsWhereFirstFallbackIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        AreEqual(1, stmf.ListTac().Count(), "should be 1 - the fallback");
    }

    [TestMethod]
    public void DoManyFoldFallback()
    {
        var stmf = AssembleTestFallbackStream(addStream1: false);
        //stmf.InForTests().Remove("1");
        var inDefaultStream = stmf.InTac()[StreamDefaultName];
        stmf.Attach("Fallback1", inDefaultStream);
        stmf.Attach("Fallback2", inDefaultStream);
        stmf.Attach("Fallback3", inDefaultStream);
        stmf.Attach("Fallback4", inDefaultStream);
        stmf.Attach("Fallback5", inDefaultStream);

        AreEqual(45, stmf.ListTac().Count(), "Should have looped through many and found the 45");
        AreEqual("ZMany", stmf.ReturnedStreamName);
    }

    [TestMethod]
    public void DoManyFallbacks2()
    {
        var sourceForStream1 = AssembleTestFallbackStream(addStream1: true);
        var stmf = AssembleTestFallbackStream(addStream1: false);
        stmf.Attach("ZZZ", sourceForStream1.InTac()["1"]); // should be after the "ZMany" so it should not return anything
        stmf.Attach("1", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("2", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("3", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("Fallback5", stmf.InTac()[StreamDefaultName]);
        AreEqual(45, stmf.ListTac().Count(), "Should have looped through many and found the 45");
        AreEqual("ZMany", stmf.ReturnedStreamName);
    }

    [TestMethod]
    public void FindNothing()
    {
        var stmf = AssembleTestFallbackStream(addStream1: false, addStreamZMany: false);
        stmf.Attach("1", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("2", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("3", stmf.InTac()[StreamDefaultName]);
        AreEqual(0, stmf.ListTac().Count(), "Should find none");
    }

    private StreamFallback AssembleTestFallbackStream(bool addStream1 = true, bool addStreamZMany = true)
    {
        var emptyDs = new DataTablePerson(this).Generate(0, 1001);
        var streams = DsSvc.CreateDataSource<StreamFallback>(emptyDs);

        var dsWith1 = new DataTablePerson(this).Generate(1, 2000);
        var dsWithmany = new DataTablePerson(this).Generate(45, 4000);
        if (addStream1)
            streams.AttachTac("1", dsWith1);
        if (addStreamZMany)
            streams.AttachTac("ZMany", dsWithmany);
        return streams;

    }
}