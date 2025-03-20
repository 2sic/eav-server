namespace ToSic.Eav.DataSources.Attributes;

public partial class AttributeRenameTests
{
    private const string MapDropTitle = @"First=FirstName";


    [Fact]
    public void TitleChange()
    {
        var test = attributeRenameTester.Init(MapDropTitle, false);
        Equal(10, test.CList.Count);
        Single(test.CItem.Attributes); //, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, PersonSpecs.Fields, [ShortFirst]);

        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);

        Null(test.CItem.GetBestTitle());//, "should get null title?");
        Null(test.CItem.Title);//, "title attribute will be null");
    }



}