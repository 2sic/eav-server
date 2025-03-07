using ToSic.Eav.Apps;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSourceTests;

[Startup(typeof(TestStartupEavCoreAndDataSources))]
public class ItemFilterDuplicatesTest(DataSourcesTstBuilder dsSvc, Generator<DataTablePerson> personTableGenerator, IDataSourcesService dataSourcesService)
{
    [Fact]
    public void ItemFilterDuplicates_In0()
    {
        var desiredFinds = 0;
        var sf = dsSvc.CreateDataSource<ItemFilterDuplicates>();
        var found = sf.ListTac().Count();
        Equal(desiredFinds, found);//, "Should find exactly this amount people");

        var dupls = sf[ItemFilterDuplicates.DuplicatesStreamName].ListTac().Count();
        Equal(desiredFinds, dupls);//, "Should find exactly this amount people");
    }

    [Fact]
    public void StreamMerge_In1()
    {
        var desiredFinds = 100;
        var desiredDupls = 0;
        var sf = GenerateDuplsDs(desiredFinds, 1);
        var found = sf.ListTac().Count();
        Equal(desiredFinds, found);//, "Should find exactly this amount people");

        var dupls = sf[ItemFilterDuplicates.DuplicatesStreamName].ListTac().Count();
        Equal(desiredDupls, dupls);//, "Should find exactly this amount people");
    }

    [Fact]
    public void StreamMerge_In2()
    {
        // fi
        var items = 100;
        var desiredFinds = items;
        var desiredDupls = items;
        var sf = GenerateDuplsDs(items, 2);

        var found = sf.ListTac().Count();
        Equal(desiredFinds, found);//, "Should find exactly this amount people");

        var dupls = sf[ItemFilterDuplicates.DuplicatesStreamName].ListTac().Count();
        Equal(desiredDupls, dupls);//, "Should find exactly this amount people");
    }

    [Fact]
    public void StreamMerge_In3()
    {
        // fi
        var items = 100;
        var desiredFinds = items;
        var desiredDupls = items;
        var sf = GenerateDuplsDs(items, 3);

        var found = sf.ListTac().Count();
        Equal(desiredFinds, found);//, "Should find exactly this amount people");

        var dupls = sf[ItemFilterDuplicates.DuplicatesStreamName].ListTac().Count();
        Equal(desiredDupls, dupls);//, "Should find exactly this amount people");
    }



    private ItemFilterDuplicates GenerateDuplsDs(int desiredFinds, int attach)
    {
        if (attach < 1) throw new("attach must be at least 1");
        var ds = personTableGenerator.New().Generate(desiredFinds, 1001, true);
        var sf = dataSourcesService.CreateTac<StreamMerge>(appIdentity: new AppIdentity(0, 0), upstream: ds);

        for (int i = 1; i < attach; i++)
            sf.Attach("another" + i, ds.Out.First().Value);

        var unique = dataSourcesService.CreateTac<ItemFilterDuplicates>(appIdentity: new AppIdentity(0, 0), upstream: sf);
        return unique;
    }
}