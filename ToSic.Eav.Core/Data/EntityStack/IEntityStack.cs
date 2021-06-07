using System;
using System.Collections.Immutable;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Should be an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
    /// </summary>
    [PrivateApi("still WIP")]
    public interface IEntityStack
    {
        IImmutableList<IEntity> Stack { get; }

        Tuple<object, string, IEntity> ValueAndMore(string fieldName, string[] dimensions, bool treatEmptyAsDefault = true);

    }
}
