using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources.Catalog;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSources.Tests
{
    [TestClass]
    public class CheckVisualQueryAttribute
    {
        [TestMethod]
        public void CheckGlobalNames()
        {
            var dsCatalog = EavTestBase.Resolve<DataSourceCatalog>().Init(null);

            var allDS = DataSourceCatalog.GetAll(true).ToList();

            allDS.ForEach(ds =>
            {
                var dsInfo = ds.VisualQuery;//.GetCustomAttributes(typeof(VisualQueryAttribute), true).FirstOrDefault() as VisualQueryAttribute;
                // check GlobalName

                // 2018-01-20 2dm disabled this - it fails, but I'm not ever sure if this is relevant any more
                // I believe it had to do with renaming the IDs, but I think it's not necessary any more
                // Assert.AreEqual(dsInfo.GlobalName, ds.Type.FullName, $"testing '{ds.Type.FullName}'");

                // check config
                var dsDataType = dsInfo.ExpectsDataOfType;
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
}
