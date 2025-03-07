namespace ToSic.Eav.DataSourceTests;

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
        var test = new AttributeRenameTester(this).Init(MapBasic1);
        AreEqual(10, test.CList.Count);
        AreEqual(PersonSpecs.ValueColumns, test.CItem.Attributes.Count, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, [PersonSpecs.FieldFullName], [ShortName]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName);
    }

    [TestMethod]
    public void Map1ChangeWithDropRest()
    {
        var test = new AttributeRenameTester(this).Init(MapBasic1, false);
        AreEqual(10, test.CList.Count);
        AreNotEqual(PersonSpecs.ValueColumns, test.CItem.Attributes.Count, "expected a different amount of columns");
        AreEqual(1, test.CItem.Attributes.Count, "expect only 1 field now");

        AssertFieldsChanged(test.CItem, PersonSpecs.Fields, [ShortName]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
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
        var test = new AttributeRenameTester(this).Init(map);
        AreEqual(10, test.CList.Count);
        AreEqual(PersonSpecs.ValueColumns, test.CItem.Attributes.Count, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem,
            [PersonSpecs.FieldFullName, PersonSpecs.FieldFirstName, PersonSpecs.FieldLastName],
            [ShortName, ShortFirst, ShortLast]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);
        test.AssertValues(PersonSpecs.FieldLastName, ShortLast);
    }

    [TestMethod]
    public void Map3ChangeWithDropRest()
    {
        var test = new AttributeRenameTester(this).Init(Map3Fields, false);
        AreEqual(10, test.CList.Count);
        AreEqual(3, test.CItem.Attributes.Count, "expected the same amount of columns");

        AssertFieldsChanged(test.CItem, 
            PersonSpecs.Fields,
            [ShortName, ShortFirst, ShortLast]);

        test.AssertValues(PersonSpecs.FieldFullName, ShortName);
        test.AssertValues(PersonSpecs.FieldFirstName, ShortFirst);
        test.AssertValues(PersonSpecs.FieldLastName, ShortLast);
    }





}