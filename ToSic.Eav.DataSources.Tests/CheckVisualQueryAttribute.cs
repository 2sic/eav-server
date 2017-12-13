using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.DataSources.Tests
{
    [TestClass]
    public class CheckVisualQueryAttribute
    {
        [TestMethod]
        public void CheckGlobalNames()
        {
            var allDS = DataSource.GetInstalledDataSources2(true).ToList();

            allDS.ForEach(ds =>
            {
                var dsInfo = ds.VisualQuery;//.GetCustomAttributes(typeof(VisualQueryAttribute), true).FirstOrDefault() as VisualQueryAttribute;
                // check GlobalName
                Assert.AreEqual(dsInfo.GlobalName, ds.Type.FullName, $"testing '{ds.Type.FullName}'");

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
                        Assert.AreEqual("|Config " + ds.Type.FullName, dsDataType, $"types should be same {ds.Type.FullName}");
                    }
                }
            });
        }
    }
}
