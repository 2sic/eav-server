using System;
using System.Collections.Generic;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

internal class TestData
{
    public const string FieldName = "Name";
    public const string FieldBirthday = "Birthday";
    public const string FieldDog = "Dog";
    public const string FieldCat = "Cat";
    public const string FieldChildren = "Children";
    public const string UnusedField = "DummyUnusedField";

    #region Sub-Data - must be created before the objects which will reference them

    public static TestPropLookupData ChildJb1 = new TestPropLookupData("Person-Child1", "Child Jungleboy 1")
    {
        Birthday = new DateTime(2020, 1, 1),
    };

    public static TestPropLookupData GrandchildJb = new TestPropLookupData("Person-Child2-1", "Grandchild Jungleboy 2")
    {
        Birthday = new DateTime(2020, 2, 2),
        Dog = "Samantha",
    };


    public static TestPropLookupData ChildJb2 = new TestPropLookupData("Person-Child2", "Child Jungleboy 2")
    {
        Birthday = new DateTime(2020, 2, 2),
        Dog = "Bello",
        Children = new List<PropertyLookupDictionary>()
        {
            GrandchildJb.Lookup
        }
    };

    #endregion

    public static TestPropLookupData Jungleboy = new TestPropLookupData("Person-Jungleboy", "iJungleboy")
    {
        Birthday = new DateTime(2012, 11, 10),
        //Children = new List<IPropertyLookup>
        Children = new List<PropertyLookupDictionary>
        {
            ChildJb1.Lookup,
            ChildJb2.Lookup,
        }
    };

    public static TestPropLookupData JohnDoe = new TestPropLookupData("Person-JohnDoe", "JohnDoe")
    {
        Birthday = new DateTime(1987, 1, 2),
        Dog = "Streak",
    };

    public static TestPropLookupData ChildOfJaneDoe = new TestPropLookupData("Person-ChildOfJaneDoe", "ChildOfJaneDoe")
    {
        Name = "Child of John Doe 1",
        Birthday = new DateTime(2021, 3, 3),
        Cat = "Eowyn"
    };


    public static TestPropLookupData JaneDoeWithChildren = new TestPropLookupData("Person-JaneDoe", "JaneDoe")
    {
        Birthday = new DateTime(1987, 1, 2),
        Dog = "Strike",
        Children = new List<PropertyLookupDictionary>()
        {
            ChildOfJaneDoe.Lookup
        }
    };
}