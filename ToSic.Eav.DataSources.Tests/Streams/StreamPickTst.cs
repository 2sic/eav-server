﻿using ToSic.Eav.Apps;
using ToSic.Eav.LookUp;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSourceTests.Streams;

[TestClass]
public class StreamPickTst: TestBaseEavDataSource
{
    private const int DefaultStreamSize = 10;
    private const int MoreStreamSize = 27;
    private const string MoreStream = "More";

    [TestMethod]
    public void StreamPickDefault()
    {
        var streamPick = BuildStructure();
        var list = streamPick.ListTac();
        AreEqual(list.Count(), DefaultStreamSize, "default should have 10");
    }

    [TestMethod]
    public void StreamPickMore()
    {
        var streamPick = BuildStructure();
        streamPick.StreamName = MoreStream;
        var list = streamPick.ListTac();
        AreEqual(list.Count(), MoreStreamSize, "default should have 27");
    }


    private StreamPick BuildStructure()
    {
        // todo: create a test using params...
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, new Dictionary<string, string>
        {
            {"StreamParam", "Lots"}
        });

        var ds1 = new DataTablePerson(this).Generate(DefaultStreamSize, 1000);
        var ds2 = new DataTablePerson(this).Generate(MoreStreamSize, 2700);
        var ds3 = new DataTablePerson(this).Generate(53, 5300);
        var dsBuild = GetService<IDataSourcesService>();
        var streamPick = dsBuild.CreateTac<StreamPick>(appIdentity: new AppIdentity(1, 1), configLookUp: ds1.Configuration.LookUpEngine);
        streamPick.AttachTac(DataSourceConstants.StreamDefaultName, ds1);
        streamPick.AttachTac(MoreStream, ds2);
        streamPick.AttachTac("Lots", ds3);
        return streamPick;
    }
}