using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.TestData;

namespace ToSic.Eav.DataSourceTests
{
    public partial class AttributeRenameTests
    {
        private const string MapDropTitle = @"First=FirstName";


        [TestMethod]
        public void TitleChange()
        {
            var test = new AttributeRenameTester(this).Init(MapDropTitle, false);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreEqual(1, test.CItem.Attributes.Count, "expected the same amount of columns");

            AssertFieldsChanged(test.CItem, PersonSpecs.Fields, new[] { ShortFirst });

            test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);

            Assert.AreEqual(null, test.CItem.GetBestTitle(), "should get null title?");
            Assert.AreEqual(null, test.CItem.Title, "title attribute will be null");
        }



    }
}
