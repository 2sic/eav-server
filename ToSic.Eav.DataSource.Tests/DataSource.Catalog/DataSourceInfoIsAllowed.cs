using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Sys.Users;

namespace ToSic.Eav.DataSource.Catalog;

/// <summary>
/// Test if the check of data-source confidentiality levels works.
/// </summary>
[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class DataSourceInfoIsAllowed(DataSourceCatalog dsCatalog)
{

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void UnknownSuperUser(bool isSuper, bool expected)
    {
        var scopeInfo = dsCatalog.FindDataSourceInfo(Error.NameId, 0);
        var ok = scopeInfo.IsAllowed(new UserMock
        {
            IsSystemAdmin = isSuper
        });
        Equal(expected, ok);
    }

}
