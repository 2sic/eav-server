using System.Collections.Immutable;

namespace ToSic.Eav.Data.Source;

/// <summary>
/// An entities source which directly delivers the given entities.
/// This is almost identical wit the base class, but we should use in in code where possible, so we clearly see that
/// this is an immutable list.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImmutableEntitiesSource(IImmutableList<IEntity> entities = null)
    : DirectEntitiesSource(entities ?? ImmutableList<IEntity>.Empty);