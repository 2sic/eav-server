using System.Collections;
using System.Collections.Immutable;
using ToSic.Eav.Data.Entities.Sys.Sources;
using ToSic.Eav.Data.Relationships.Sys;
using ToSic.Eav.Data.Values.Sys;

namespace ToSic.Eav.Data.Build;

partial class ValueBuilder
{
    public IValue Relationship(List<int?> references, IEntitiesSource app)
        => Relationship(new LazyEntitiesSource(app, references));

    public IValue Relationship(IEnumerable<IEntity> directList)
        => new Value<IEnumerable<IEntity>>(directList);

    public IImmutableList<IValue> Relationships(IEnumerable<IEntity> directList)
        => new List<IValue> { new Value<IEnumerable<IEntity>>(directList) }.ToImmutableList();

    public IImmutableList<IValue> Relationships(IRelatedEntitiesValue value, IEntitiesSource app)
        => Relationships(new LazyEntitiesSource(app, value.Identifiers));

    public IValue Relationship(List<Guid?> guids, IEntitiesSource fullLookupList) => 
        new Value<IEnumerable<IEntity>>(new LazyEntitiesSource(fullLookupList, guids));

    public IValue RelationshipWip(object? value, IEntitiesSource? fullEntityListForLookup)
    {
        var rel = GetLazyEntitiesForRelationship(value, fullEntityListForLookup);
        return new Value<IEnumerable<IEntity>>(rel);
    }

    private LazyEntitiesSource GetLazyEntitiesForRelationship(object? value, IEntitiesSource? fullLookupList)
    {
        var entityIds = (value as IEnumerable<int?>)
                        ?.ToList()
                        ?? (value as IEnumerable<int>)
                        ?.Select(x => (int?)x)
                        .ToList();
        if (entityIds != null)
            return new(fullLookupList, entityIds);
        if (value is IRelatedEntitiesValue relList)
            return new(fullLookupList, relList.Identifiers);
        if (value is List<Guid?> guids)
            return new(fullLookupList, guids);
        return new(fullLookupList, GuidCsvToList(value));
    }


    private static List<Guid?> GuidCsvToList(object? value)
    {
        var entityIdEnum = value as IEnumerable; // note: strings are also enum!
        if (value is string stringValue && stringValue.HasValue())
            entityIdEnum = stringValue.CsvToArrayWithoutEmpty();
        // this is the case when we get a CSV-string with GUIDs
        var entityGuids = entityIdEnum?
                              .Cast<object>()
                              .Select(x =>
                              {
                                  var v = x?.ToString().Trim();
                                  // this is the case when an export contains a list with nulls as a special code
                                  if (v is null or Constants.EmptyRelationship)
                                      return new();
                                  var guid = Guid.Parse(v);
                                  return guid == Guid.Empty ? new Guid?() : guid;
                              })
                              .ToList()
                          ?? [];
        return entityGuids;
    }



    /// <summary>
    /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
    /// ...and then it must be a new object every time, 
    /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
    /// </summary>
    private Value<IEnumerable<IEntity>> NewEmptyRelationship
        => new(new LazyEntitiesSource(null, identifiers: null));

    internal IImmutableList<IValue> NewEmptyRelationshipValues => new List<IValue> { NewEmptyRelationship }.ToImmutableList();

}