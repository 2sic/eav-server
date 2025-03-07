using ToSic.Eav.TestData;

namespace ToSic.Eav.DataSourceTests;

public partial class AttributeRenameTests
{
    private const string MapDropTitle = @"First=FirstName";


    [TestMethod]
    public void TitleChange()
    {
        var test = new AttributeRenameTester(this).Init(MapDropTitle, false);
        AreEqual(10, test.CList.Count);
        AreEqual(1, test.CItem.Attributes.Count, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, PersonSpecs.Fields, [ShortFirst]);

        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);

        AreEqual(null, test.CItem.GetBestTitle(), "should get null title?");
        AreEqual(null, test.CItem.Title, "title attribute will be null");
    }



}