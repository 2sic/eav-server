using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.TestData;
using ToSic.Testing.Shared;

namespace ToSic.Eav.DataSourceTests
{
    // Todo
    // Create tests with language-parameters as well, as these tests ignore the language and always use default

    [TestClass]
    public class ValueFilterBoolean: TestBaseEavDataSource
    {
        private readonly ValueFilterMaker _valueFilterMaker;

        public ValueFilterBoolean()
        {
            _valueFilterMaker = new ValueFilterMaker(this);
        }

        [DataRow("True", 82, 82, true, "True - Table")]
        [DataRow("True", 82, 82, false, "True - Entity")]
        [DataRow("true", 82, 82, true, "true - Table")]
        [DataRow("true", 82, 82, false, "true - Entity")]
        [DataRow("TRUE", 82, 82, true, "TRUE - Table")]
        [DataRow("TRUE", 82, 82, false, "TRUE - Entity")]
        [DataRow("FALSE", 164, 82, true, "FALSE - Table")]
        [DataRow("FALSE", 164, 82, false, "FALSE - Entity")]
        [DataRow("false", 164, 82, true, "false - Table")]
        [DataRow("false", 164, 82, false, "false - Entity")]
        [DataTestMethod]
        public void FilterBool(string compareValue, int desiredFinds, int populationRoot, bool useTable, string name)
        {
            var vf = _valueFilterMaker.CreateValueFilterForTesting(populationRoot * PersonSpecs.IsMaleForEveryX, useTable); // only every 3rd is male in the demo data
            vf.Attribute = "IsMale";
            vf.Value = compareValue;
            var found = vf.ListForTests().Count();
            Assert.AreEqual(desiredFinds, found, "Should find exactly this amount people");
        }

    }
}
