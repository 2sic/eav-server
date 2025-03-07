using ToSic.Lib.DI;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSourceTests.Streams;

[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class StreamFallbackTst(DataSourcesTstBuilder DsSvc, Generator<DataTablePerson> personTableGenerator)
{
    [Fact]
    public void StreamWhereDefaultIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        stmf.Attach(StreamDefaultName, stmf.InTac()["1"]);

        Equal(1, stmf.ListTac().Count()); //, "should have found 1");
    }

    [Fact]
    public void StreamsWhereFirstFallbackIsReturned()
    {
        var stmf = AssembleTestFallbackStream();
        Equal(1, stmf.ListTac().Count()); //, "should be 1 - the fallback");
    }

    [Fact]
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

        Equal(45, stmf.ListTac().Count()); //, "Should have looped through many and found the 45");
        Equal("ZMany", stmf.ReturnedStreamName);
    }

    [Fact]
    public void DoManyFallbacks2()
    {
        var sourceForStream1 = AssembleTestFallbackStream(addStream1: true);
        var stmf = AssembleTestFallbackStream(addStream1: false);
        stmf.Attach("ZZZ", sourceForStream1.InTac()["1"]); // should be after the "ZMany" so it should not return anything
        stmf.Attach("1", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("2", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("3", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("Fallback5", stmf.InTac()[StreamDefaultName]);
        Equal(45, stmf.ListTac().Count()); //, "Should have looped through many and found the 45");
        Equal("ZMany", stmf.ReturnedStreamName);
    }

    [Fact]
    public void FindNothing()
    {
        var stmf = AssembleTestFallbackStream(addStream1: false, addStreamZMany: false);
        stmf.Attach("1", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("2", stmf.InTac()[StreamDefaultName]);
        stmf.Attach("3", stmf.InTac()[StreamDefaultName]);
        Equal(0, stmf.ListTac().Count()); //, "Should find none");
    }

    private StreamFallback AssembleTestFallbackStream(bool addStream1 = true, bool addStreamZMany = true)
    {
        var emptyDs = personTableGenerator.New() /*new DataTablePerson(this)*/.Generate(0, 1001);
        var streams = DsSvc.CreateDataSource<StreamFallback>(emptyDs);

        var dsWith1 = personTableGenerator.New() /*new DataTablePerson(this)*/.Generate(1, 2000);
        var dsWithmany = personTableGenerator.New() /*new DataTablePerson(this)*/.Generate(45, 4000);
        if (addStream1)
            streams.AttachTac("1", dsWith1);
        if (addStreamZMany)
            streams.AttachTac("ZMany", dsWithmany);
        return streams;

    }
}