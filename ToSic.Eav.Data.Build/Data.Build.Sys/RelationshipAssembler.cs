using System.Collections;
using System.Collections.Immutable;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.Sys.Values;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Internal assembler to create relationship values.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class RelationshipAssembler
{
    public IEnumerable<IEntity?> ToSource(ICollection<int?> references, IEntitiesSource app)
        => new LazyEntitiesSource(app, references);

    public IEnumerable<IEntity?> ToSource(ICollection<Guid?> guids, IEntitiesSource? fullLookupList)
        => new LazyEntitiesSource(fullLookupList, guids);

    public IEnumerable<IEntity?> ToSource(IRelatedEntitiesValue value, IEntitiesSource app)
        => new LazyEntitiesSource(app, value.Identifiers);

    public IEnumerable<IEntity?> ToSource(IEnumerable<object> keys, ILookup<object, IEntity> lookup)
        => new LookUpEntitiesSource<object>(keys, lookup);



    public IValue Relationship(IEnumerable<IEntity?> directList)
        => new Value<IEnumerable<IEntity?>>(directList);

    /// <summary>
    /// Quick internal method to create, should never be public as it could require DI some day
    /// </summary>
    /// <param name="directList"></param>
    /// <returns></returns>
    [PrivateApi]
    internal static IValue RelationshipQ(IEnumerable<IEntity?> directList)
        => new Value<IEnumerable<IEntity?>>(directList);

    /// <summary>
    /// Generate a new empty relationship. This is important, because it's used often to create empty relationships...
    /// ...and then it must be a new object every time, 
    /// because the object could be changed at runtime, and if it were shared, then it would be changed in many places
    /// </summary>
    private static Value<IEnumerable<IEntity?>> NewEmptyRelationship
        => new(new LazyEntitiesSource(null, identifiers: Array.Empty<int?>()));

    internal static IImmutableList<IValue> NewEmptyRelationshipValues =>
        new List<IValue> { NewEmptyRelationship }.ToImmutableOpt();


    internal static LazyEntitiesSource GetLazyEntitiesForRelationshipWip(object? value, IEntitiesSource? fullLookupList)
    {
        var entityIds = (value as IEnumerable<int?>)?.ToListOpt()
                        ?? (value as IEnumerable<int>)?.Select(x => (int?)x).ToListOpt();
        return entityIds != null
            ? new(fullLookupList, entityIds)
            : value switch
            {
                IRelatedEntitiesValue relList => new(fullLookupList, relList.Identifiers),
                ICollection<Guid?> guids => new(fullLookupList, guids),
                _ => new(fullLookupList, GuidCsvToList(value))
            };
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
}
