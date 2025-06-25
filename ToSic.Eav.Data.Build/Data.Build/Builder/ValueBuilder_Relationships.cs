﻿using System.Collections;
using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Data.Build;

partial class ValueBuilder
{
    public IValue Relationship(ICollection<int?> references, IEntitiesSource app)
        => Relationship(new LazyEntitiesSource(app, references));

    public IValue Relationship(IEnumerable<IEntity?> directList)
        => new Value<IEnumerable<IEntity?>>(directList);

    public IImmutableList<IValue> Relationships(IEnumerable<IEntity?> directList)
        => new List<IValue> { new Value<IEnumerable<IEntity?>>(directList) }.ToImmutableOpt();

    public IImmutableList<IValue> Relationships(IRelatedEntitiesValue value, IEntitiesSource app)
        => Relationships(new LazyEntitiesSource(app, value.Identifiers));

    public IValue Relationship(ICollection<Guid?> guids, IEntitiesSource? fullLookupList)
        => new Value<IEnumerable<IEntity?>>(new LazyEntitiesSource(fullLookupList, guids));

    public IValue RelationshipWip(object? value, IEntitiesSource? fullEntityListForLookup)
    {
        var rel = GetLazyEntitiesForRelationship(value, fullEntityListForLookup);
        return new Value<IEnumerable<IEntity?>>(rel);
    }

    private LazyEntitiesSource GetLazyEntitiesForRelationship(object? value, IEntitiesSource? fullLookupList)
    {
        var entityIds = (value as IEnumerable<int?>)?.ToListOpt()
                        ?? (value as IEnumerable<int>)?.Select(x => (int?)x).ToListOpt();
        if (entityIds != null)
            return new(fullLookupList, entityIds);
        if (value is IRelatedEntitiesValue relList)
            return new(fullLookupList, relList.Identifiers);
        if (value is ICollection<Guid?> guids)
            return new(fullLookupList, guids);
        return new(fullLookupList, GuidCsvToList(value));
    }


    private static ICollection<Guid?> GuidCsvToList(object? value)
    {
        var entityIdEnum = value as IEnumerable; // note: strings are also enum!
        if (value is string stringValue && stringValue.HasValue())
            entityIdEnum = stringValue.CsvToArrayWithoutEmpty();
        // this is the case when we get a CSV-string with GUIDs
        var entityGuids = entityIdEnum?
                              .Cast<object>()
                              .Select(x =>
                              {
                                  // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                                  var v = x?.ToString()?.Trim();
                                  // this is the case when an export contains a list with nulls as a special code
                                  if (v is null or EavConstants.EmptyRelationship)
                                      return new();
                                  var guid = Guid.Parse(v);
                                  return guid == Guid.Empty
                                      ? new Guid?()
                                      : guid;
                              })
                              .ToListOpt()
                          ?? [];
        return entityGuids;
    }



    /// <summary>
    /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
    /// ...and then it must be a new object every time, 
    /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
    /// </summary>
    private Value<IEnumerable<IEntity?>> NewEmptyRelationship
        => new(new LazyEntitiesSource(null, identifiers: Array.Empty<int?>()));

    internal IImmutableList<IValue> NewEmptyRelationshipValues => new List<IValue> { NewEmptyRelationship }.ToImmutableOpt();

}