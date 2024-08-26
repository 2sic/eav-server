using System.Collections.Generic;
using System;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Helpers;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

internal class TestPropLookupData
{
    public string SourceId { get; }
    public string Name;
    public DateTime Birthday;
    public string Dog;
    public string Cat;


    //public IEnumerable<IPropertyLookup> Children;
    public IEnumerable<PropertyLookupDictionary> Children;

    public TestPropLookupData(string sourceId, string name)
    {
        SourceId = sourceId;
        Name = name;
    }

    private Dictionary<string, object> Data()
    {
        var values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            { FieldName, Name },
            { FieldBirthday, Birthday },
        };
        if (Dog != null) values.Add(FieldDog, Dog);
        if (Children != null) values.Add(FieldChildren, Children);
        if (Cat != null) values.Add(FieldCat, Cat);
        return values;
    }

    public PropertyLookupDictionary Lookup => _lookup.Get(() => new PropertyLookupDictionary(SourceId, Data()));
    private readonly GetOnce<PropertyLookupDictionary> _lookup = new();

    public KeyValuePair<string, IPropertyLookup> StackPart =>
        _stackPart.Get(() => new KeyValuePair<string, IPropertyLookup>(Lookup.NameId, Lookup));

    private readonly GetOnce<KeyValuePair<string, IPropertyLookup>> _stackPart =
        new GetOnce<KeyValuePair<string, IPropertyLookup>>();

}