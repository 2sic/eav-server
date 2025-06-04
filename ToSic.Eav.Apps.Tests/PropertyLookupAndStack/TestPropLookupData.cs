using System;
using ToSic.Eav.Data.Sys;
using ToSic.Lib.Helpers;
using static ToSic.Eav.Apps.Tests.PropertyLookupAndStack.TestData;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

internal class TestPropLookupData(string sourceId, string name)
{
    public string SourceId { get; } = sourceId;
    public string Name = name;
    public DateTime Birthday;
    public string Dog;
    public string Cat;


    //public IEnumerable<IPropertyLookup> Children;
    public IEnumerable<PropertyLookupDictionary> Children;

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

    public PropertyLookupDictionary Lookup => _lookup.Get(() => new(SourceId, Data()));
    private readonly GetOnce<PropertyLookupDictionary> _lookup = new();

    public KeyValuePair<string, IPropertyLookup> StackPart =>
        _stackPart.Get(() => new(Lookup.NameId, Lookup));

    private readonly GetOnce<KeyValuePair<string, IPropertyLookup>> _stackPart = new();

}