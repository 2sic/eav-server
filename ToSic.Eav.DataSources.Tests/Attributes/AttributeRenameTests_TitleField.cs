using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Attributes
{
    public partial class AttributeRenameTests
    {
        private const string MapDropTitle = @"First=FirstName";


        [TestMethod]
        public void TitleChange()
        {
            var test = new AttributeRenameTester(MapDropTitle, false);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreEqual(1, test.CItem.Attributes.Count, "expected the same amount of columns");

            AssertFieldsChanged(test.CItem, DataTableTst.Fields, new[] { ShortFirst });

            test.AssertValues(DataTableTst.FieldFirstName, ShortFirst);

            Assert.AreEqual(null, test.CItem.GetBestTitle(), "should get null title?");
            Assert.AreEqual(null, test.CItem.Title, "title attribute will be null");
        }



    }
}
