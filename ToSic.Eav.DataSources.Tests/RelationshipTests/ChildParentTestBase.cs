using System.Collections.Generic;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests;

public class ChildParentTestBase<T> : TestBaseDiEavFullAndDb where T: RelationshipDataSourceBase
{
    private DataSourcesTstBuilder DsSvc => field ??= GetService<DataSourcesTstBuilder>();

    protected T PrepareDsWithOptions(string appType = null, IEnumerable<int> ids = null, ILookUpEngine lookUpEngine = null, object optionsForLastDs = null)
    {
        lookUpEngine ??= new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();

        var baseDs = DsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppIdentity,
            LookUp = lookUpEngine,
        });
        var appDs = DsSvc.CreateDataSource<App>(baseDs);
        var inStream = appDs.GetStream(appType);
        inStream = FilterStreamByIds(ids, inStream);

        var childDs = DsSvc.CreateDataSource<T>(attach: inStream, options: optionsForLastDs);

        return childDs;
    }
    protected T PrepareDs(string appType = null, IEnumerable<int> ids = null, string fieldName = null, ILookUpEngine lookUpEngine = null)
    {
        lookUpEngine ??= new LookUpTestData(GetService<DataBuilder>()).AppSetAndRes();

        var baseDs = DsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppIdentity,
            LookUp = lookUpEngine,
        });
        var appDs = DsSvc.CreateDataSource<App>(baseDs);
        var inStream = appDs.GetStream(appType);
        inStream = FilterStreamByIds(ids, inStream);

        var childDs = DsSvc.CreateDataSource<T>(inStream, options: fieldName == null ? null : new { FieldName = fieldName });

        return childDs;
    }
}