namespace ToSic.Eav.Data.Sys.Entities.Sources;

/// <summary>
/// An entities source which directly delivers the given entities.
/// This is almost identical wit the base class, but we should use in code where possible, so we clearly see that
/// this is an immutable list.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImmutableEntitiesSource(IReadOnlyCollection<IEntity>? entities = null)
    : DirectEntitiesSource(entities ?? []);