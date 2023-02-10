using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    [TestClass]
    public class GetThisTests: TestBaseDiEavFullAndDb
    {
        [TestMethod] public void GetThisString() => AreEqual(TestDsGetThis.ExpectedGetThisString, GetPropsDs().GetThisString);

        [TestMethod] public void GetThisTrue() => AreEqual(true, GetPropsDs().GetThisTrue);

        [TestMethod] public void GetThisFalseDefault() => AreEqual(false, GetPropsDs().GetThisFalseDefault);
        [TestMethod] public void GetThisFalseInitialized() => AreEqual(false, GetPropsDs().GetThisFalseInitialized);


        private TestDsGetThis GetPropsDs()
        {
            var ds = this.GetTestDataSource<TestDsGetThis>(LookUpTestData.EmptyLookupEngine);
            ds.Configuration.Parse();
            return ds;
        }
    }
}
