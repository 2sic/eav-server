using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSourceTests.Attributes
{
    public partial class AttributeRenameTests
    {
        private const string ShortName = "Name";
        private const string ShortFirst = "First";
        private const string ShortLast = "Last";
        private const string MapBasic1 = @"Name=FullName";
        private const string Map3Fields = @"Name=FullName
First=FirstName
Last=LastName";

        [TestMethod]
        public void Map1ChangeWithPreserve()
        {
            var test = new AttributeRenameTester(MapBasic1);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreEqual(DataTableTst.ValueColumns, test.CItem.Attributes.Count, "expected the same amount of columns");

            AssertFieldsChanged(test.CItem, new []{DataTableTst.FieldFullName}, new []{ShortName});

            test.AssertValues(DataTableTst.FieldFullName, ShortName);
            test.AssertValues(DataTableTst.FieldFirstName);
        }

        [TestMethod]
        public void Map1ChangeWithDropRest()
        {
            var test = new AttributeRenameTester(MapBasic1, false);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreNotEqual(DataTableTst.ValueColumns, test.CItem.Attributes.Count, "expected a different amount of columns");
            Assert.AreEqual(1, test.CItem.Attributes.Count, "expect only 1 field now");

            AssertFieldsChanged(test.CItem, DataTableTst.Fields, new[] { ShortName });

            test.AssertValues(DataTableTst.FieldFullName, ShortName);
        }

        [TestMethod]
        public void Map3ChangeWithPreserve() => Map3ChangeWithPreserve(Map3Fields);

        /// <summary>
        /// This test should result in the same thing as the original test, because the other field doesn't exist
        /// </summary>
        [TestMethod]
        public void Map3PlusFaultyWithPreserve() => Map3ChangeWithPreserve(Map3Fields + "\nNewName=DoesNotExist");

        public void Map3ChangeWithPreserve(string map)
        {
            var test = new AttributeRenameTester(map);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreEqual(DataTableTst.ValueColumns, test.CItem.Attributes.Count, "expected the same amount of columns");

            AssertFieldsChanged(test.CItem,
                new[] { DataTableTst.FieldFullName, DataTableTst.FieldFirstName, DataTableTst.FieldLastName },
                new[] { ShortName, ShortFirst, ShortLast });

            test.AssertValues(DataTableTst.FieldFullName, ShortName);
            test.AssertValues(DataTableTst.FieldFirstName, ShortFirst);
            test.AssertValues(DataTableTst.FieldLastName, ShortLast);
        }

        [TestMethod]
        public void Map3ChangeWithDropRest()
        {
            var test = new AttributeRenameTester(Map3Fields, false);
            Assert.AreEqual(10, test.CList.Count);
            Assert.AreEqual(3, test.CItem.Attributes.Count, "expected the same amount of columns");

            AssertFieldsChanged(test.CItem, 
                DataTableTst.Fields,
                new[] {ShortName, ShortFirst, ShortLast});

            test.AssertValues(DataTableTst.FieldFullName, ShortName);
            test.AssertValues(DataTableTst.FieldFirstName, ShortFirst);
            test.AssertValues(DataTableTst.FieldLastName, ShortLast);
        }





    }
}
