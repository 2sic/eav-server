using System.Collections.Immutable;

namespace ToSic.Eav.Data.Source
{
    /// <summary>
    /// An entities source which directly delivers the given entities.
    /// This is almost identical wit the base class, but we should use in in code where possible, so we clearly see that
    /// this is an immutable list.
    /// </summary>
    public class ImmutableEntitiesSource : DirectEntitiesSource
    {
        public ImmutableEntitiesSource(IImmutableList<IEntity> entities = null)
            : base(entities ?? ImmutableList<IEntity>.Empty)
        {
        }
    }
}