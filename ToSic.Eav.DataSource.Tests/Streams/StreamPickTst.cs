using ToSic.Eav.Apps;
using ToSic.Eav.Services;
using ToSic.Lib.LookUp.Sources;

namespace ToSic.Eav.DataSourceTests.Streams;

[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class StreamPickTst(IDataSourcesService dsBuild, Generator<DataTablePerson> personTableGenerator)
{
    private const int DefaultStreamSize = 10;
    private const int MoreStreamSize = 27;
    private const string MoreStream = "More";

    [Fact]
    public void StreamPickDefault()
    {
        var streamPick = BuildStructure();
        var list = streamPick.ListTac();
        Equal(list.Count(), DefaultStreamSize); //, "default should have 10");
    }

    [Fact]
    public void StreamPickMore()
    {
        var streamPick = BuildStructure();
        streamPick.StreamName = MoreStream;
        var list = streamPick.ListTac();
        Equal(list.Count(), MoreStreamSize); //, "default should have 27");
    }


    private StreamPick BuildStructure()
    {
        // todo: create a test using params...
        var paramsOverride = new LookUpInDictionary(DataSourceConstants.ParamsSourceName, new Dictionary<string, string>
        {
            {"StreamParam", "Lots"}
        });

        var ds1 = personTableGenerator.New().Generate(DefaultStreamSize, 1000);
        var ds2 = personTableGenerator.New().Generate(MoreStreamSize, 2700);
        var ds3 = personTableGenerator.New().Generate(53, 5300);
        //var dsBuild = GetService<IDataSourcesService>();
        var streamPick = dsBuild.CreateTac<StreamPick>(appIdentity: new AppIdentity(1, 1), configLookUp: ds1.Configuration.LookUpEngine);
        streamPick.AttachTac(DataSourceConstants.StreamDefaultName, ds1);
        streamPick.AttachTac(MoreStream, ds2);
        streamPick.AttachTac("Lots", ds3);
        return streamPick;
    }
}