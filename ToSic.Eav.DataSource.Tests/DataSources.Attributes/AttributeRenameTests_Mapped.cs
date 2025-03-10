namespace ToSic.Eav.DataSources.Attributes;

public partial class AttributeRenameTests
{
    private const string ShortName = "Name";
    private const string ShortFirst = "First";
    private const string ShortLast = "Last";
    private const string MapBasic1 = @"Name=FullName";
    private const string Map3Fields = @"Name=FullName
First=FirstName
Last=LastName";

    [Fact]
    public void Map1ChangeWithPreserve()
    {
        var test = attributeRenameTester.Init(MapBasic1);
        Equal(10, test.CList.Count);
        Equal(PersonSpecs.ValueColumns, test.CItem.Attributes.Count);//, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, [PersonSpecs.FieldFullName], [ShortName]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName);
    }

    [Fact]
    public void Map1ChangeWithDropRest()
    {
        var test = attributeRenameTester.Init(MapBasic1, false);
        Equal(10, test.CList.Count);
        NotEqual(PersonSpecs.ValueColumns, test.CItem.Attributes.Count);//, "expected a different amount of columns");
        Equal(1, test.CItem.Attributes.Count);//, "expect only 1 field now");

        AssertFieldsChanged(test.CItem, PersonSpecs.Fields, [ShortName]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
    }

    [Fact]
    public void Map3ChangeWithPreserve() => Map3ChangeWithPreserveWithMap(Map3Fields);

    /// <summary>
    /// This test should result in the same thing as the original test, because the other field doesn't exist
    /// </summary>
    [Fact]
    public void Map3PlusFaultyWithPreserve() => Map3ChangeWithPreserveWithMap(Map3Fields + "\nNewName=DoesNotExist");

    private void Map3ChangeWithPreserveWithMap(string map)
    {
        var test = attributeRenameTester.Init(map);
        Equal(10, test.CList.Count);
        Equal(PersonSpecs.ValueColumns, test.CItem.Attributes.Count);//, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem,
            [PersonSpecs.FieldFullName, PersonSpecs.FieldFirstName, PersonSpecs.FieldLastName],
            [ShortName, ShortFirst, ShortLast]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);
        test.AssertValues(PersonSpecs.FieldLastName, ShortLast);
    }

    [Fact]
    public void Map3ChangeWithDropRest()
    {
        var test = attributeRenameTester.Init(Map3Fields, false);
        Equal(10, test.CList.Count);
        Equal(3, test.CItem.Attributes.Count);//, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, 
            PersonSpecs.Fields,
            [ShortName, ShortFirst, ShortLast]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);
        test.AssertValues(PersonSpecs.FieldLastName, ShortLast);
    }





}