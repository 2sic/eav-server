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

        object Value(string fieldName, bool treatEmptyAsDefault = true);

        T Value<T>(string fieldName);

        T Value<T>(string fieldName, bool treatEmptyAsDefault);
    }
}
