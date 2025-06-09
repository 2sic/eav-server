namespace ToSic.Eav.Data.Entities.Sys.Sources;

/// <summary>
/// An entities source which directly delivers the given entities.
/// This is almost identical wit the base class, but we should use in in code where possible, so we clearly see that
/// this is an immutable list.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImmutableEntitiesSource(IReadOnlyCollection<IEntity> entities = null)
    : DirectEntitiesSource(entities ?? []);