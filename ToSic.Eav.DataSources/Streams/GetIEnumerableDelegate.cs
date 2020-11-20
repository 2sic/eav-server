using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// The light list for the series of items returned
    /// </summary>
    /// <returns></returns>
    public delegate IEnumerable<IEntity> GetIEnumerableDelegate();

    public delegate IImmutableList<IEntity> GetImmutableListDelegate();

    public delegate ImmutableArray<IEntity> GetImmutableArrayDelegate();
}
