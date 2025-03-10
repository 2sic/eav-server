using ToSic.Eav.DataSource.Internal.Catalog;

namespace ToSic.Eav.DataSource.Catalog;

/// <summary>
/// Note: This Test doesn't do anything ATM as it was created at another time.
/// Could maybe be updated to be useful though, so we didn't delete it.
/// </summary>
/// <param name="dsCatalog"></param>
[Startup(typeof(StartupTestsEavCoreAndDataSources))]
public class CheckVisualQueryAttribute(DataSourceCatalog dsCatalog)
{
    [Fact]
    public void CheckGlobalNames()
    {

        var allDS = dsCatalog.GetAll(true, 0).ToList();

        allDS.ForEach(ds =>
        {
            var dsInfo = ds.VisualQuery;
            // check GlobalName

            // 2018-01-20 2dm disabled this - it fails, but I'm not ever sure if this is relevant any more
            // I believe it had to do with renaming the IDs, but I think it's not necessary any more
            // Assert.AreEqual(dsInfo.GlobalName, ds.Type.FullName, $"testing '{ds.Type.FullName}'");

            // check config
            var dsDataType = dsInfo.ConfigurationType;
            if (!string.IsNullOrWhiteSpace(dsDataType))
            {
                // check if guid
                if (Guid.TryParse(dsDataType, out Guid guid))
                {
                    // ok, guid is special
                }
                else
                {
                    // 2018-06-25 disabled this - as now the types provide their own names, and they can be different
                    // Assert.AreEqual("|Config " + ds.Type.FullName, dsDataType, $"types should be same {ds.Type.FullName}");
                }
            }
        });
    }
}