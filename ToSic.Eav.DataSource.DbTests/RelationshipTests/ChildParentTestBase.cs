using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Internal;
using ToSic.Eav.LookUp;
using static ToSic.Eav.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.RelationshipTests;

public class ChildParentTestBase<T>(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder) where T: RelationshipDataSourceBase
{
    protected T PrepareDsWithOptions(string? appType = null, IEnumerable<int>? ids = null, ILookUpEngine? lookUpEngine = null, object? optionsForLastDs = null)
    {
        lookUpEngine ??= new LookUpTestData(dataBuilder).AppSetAndRes();

        var baseDs = dsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppIdentity,
            LookUp = lookUpEngine,
        });
        var appDs = dsSvc.CreateDataSource<App>(baseDs);
        var inStream = appDs.GetStream(appType);
        inStream = dsSvc.FilterStreamByIds(ids, inStream);

        var childDs = dsSvc.CreateDataSource<T>(attach: inStream, options: optionsForLastDs);

        return childDs;
    }
    protected T PrepareDs(string? appType = null, IEnumerable<int>? ids = null, string? fieldName = null, ILookUpEngine? lookUpEngine = null)
    {
        lookUpEngine ??= new LookUpTestData(dataBuilder).AppSetAndRes();

        var baseDs = dsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions
        {
            AppIdentityOrReader = AppIdentity,
            LookUp = lookUpEngine,
        });
        var appDs = dsSvc.CreateDataSource<App>(baseDs);
        var inStream = appDs.GetStream(appType);
        inStream = dsSvc.FilterStreamByIds(ids, inStream);

        var childDs = dsSvc.CreateDataSource<T>(inStream, options: fieldName == null ? null : new { FieldName = fieldName });

        return childDs;
    }
}