using ToSic.Eav.DataSource.Sys.Catalog;
using ToSic.Eav.DataSource.VisualQuery;

namespace ToSic.Eav.DataSource.Catalog;

/// <summary>
/// Test if the check of data-source confidentiality levels works.
/// </summary>
[Startup(typeof(StartupCoreDataSourcesAndTestData))]
public class DataConfidentialityTests(DataSourceCatalog dsCatalog)
{
    [Theory]
    [InlineData(Error.NameId, DataConfidentiality.Unknown)]
    [InlineData(PublishingFilter.NameId, DataConfidentiality.Public)]
    public void ErrorDataSourceConfidentialityUnknown(string name, DataConfidentiality expected)
    {
        var scopeInfo = dsCatalog.FindDataSourceInfo(name, 0);
        var confidentiality = scopeInfo!.VisualQuery!.DataConfidentiality;
        Equal(expected, confidentiality);
    }

}
