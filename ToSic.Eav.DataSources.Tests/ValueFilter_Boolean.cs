using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSources.Tests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterBoolean
    {
        //private const int TestVolume = 10000;
        //private ValueFilter _testDataGeneratedOutsideTimer;
        //public ValueFilterBoolean()
        //{
        //    //_testDataGeneratedOutsideTimer = ValueFilter_Test.CreateValueFilterForTesting(TestVolume);
        //}
        

        #region bool tests

        public void FilterBool(string compareValue, int desiredFinds, int populationRoot)
        {
            var vf = ValueFilterString.CreateValueFilterForTesting(populationRoot * DataTableDataSourceTest.IsMaleForEveryX); // only every 3rd is male in the demo data
            vf.Attribute = "IsMale";
            vf.Value = compareValue;
            var found = vf.LightList.Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

        [TestMethod]
        public void ValueFilter_FilterBool()
        {
            FilterBool(true.ToString(), 82, 82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing1()
        {
            FilterBool("true", 82, 82);
        }
        [TestMethod]
        public void ValueFilter_FilterBoolCasing2()
        {
            FilterBool("TRUE", 82,82);
        }

        [TestMethod]
        public void ValueFilter_FilterBoolCasing3()
        {
            FilterBool("FALSE", 164, 82);
        }
        #endregion
        
    }
}
