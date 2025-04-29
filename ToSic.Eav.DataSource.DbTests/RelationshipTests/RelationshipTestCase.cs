using System.Diagnostics;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

internal class RelationshipTestCase : RelationshipTestBase
{
    internal string Name;
    internal RelationshipFilter Api;
    internal RelationshipFilter Config;

    internal IEnumerable<IEntity> All => Api.InTac()[DataSourceConstants.StreamDefaultName].ListTac();
    internal int CountAll => All.Count();

    internal int CountApi => Api.ListTac().Count();
    internal int CountConfig => Config.ListTac().Count();


    public string Type, 
        Relationship, 
        Filter, 
        RelatedAttribute, 
        CompareMode, 
        Separator, 
        Direction;

    internal RelationshipTestCase(
        DataSourcesTstBuilder dsSvc,
        DataBuilder dataBuilder,
        string name, 
        string type, 
        string relationship = null, 
        string filter = null, 
        string relAttribute = null,
        string compareMode = null,
        string separator = null,
        string direction = null)
    : base(dsSvc, dataBuilder)
    {
        Name = name;
        Type = type;
        Relationship = relationship;
        Filter = filter;
        RelatedAttribute = relAttribute;
        CompareMode = compareMode;
        Separator = separator;
        Direction = direction;
    }

    private RelationshipTestCase BuildObjects()
    {
        // test using api configuration
        Api = FilterWithApi(Type, 
            Relationship, 
            Filter,
            relAttrib: RelatedAttribute,
            compareMode:CompareMode,
            separator: Separator,
            direction: Direction);

        // Identical test with configuration providing
        Config = FilterWithConfig(Type,
            Relationship,
            Filter,
            relAttrib: RelatedAttribute,
            compareMode: CompareMode,
            separator: Separator,
            direction: Direction);

        return this;
    }

    internal void Run(bool expectsResults, bool shouldReturnAll = false, int exactCount = -1)
    {
        BuildObjects();

        var x = CountApi + CountConfig; // access the streams to ensure it's logged
        Trace.Write("Log after accessing DSs\n\n" + Log.Dump());

        Api.ListTac().ToList().ForEach(e => Trace.WriteLine($"item ({e.EntityId}):'{e.GetBestTitle()}'"));

        True(expectsResults ? CountApi > 0 : CountApi == 0, $"test: {Name} - found-Count:{CountApi} > 0");
        if(exactCount != -1)
            Equal(exactCount, CountApi);//, $"test: {Name} - missed expected exact count");

        if (shouldReturnAll)
            Equal(CountApi, CountAll);//, $"test: {Name} - all == count not met");
        else
            True(CountApi < CountAll, $"test: {Name} - foundCount < allComps");

        Equal(CountApi, CountConfig);//, $"test: {Name} - api and config should be the same");
    }
}