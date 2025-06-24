using System.Collections;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;

namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Metadata of an item (a content-type or another entity). <br/>
/// It's usually on a <strong>Metadata</strong> property of things that can have metadata.
/// </summary>
/// <typeparam name="T">The type this metadata uses as a key - int, string, guid</typeparam>
/// <remarks>
/// * Since v15.04 fully #immutable
/// </remarks>
partial class Metadata<T>
{

    #region GetBestValue

    /// <inheritdoc />
    public TVal? GetBestValue<TVal>(string name, string? typeName = null)
    {
        var list = typeName == null
            ? MetadataWithoutPermissions
            : OfType(typeName);
        var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
        return found == null
            ? default
            : found.Get<TVal>(name);
    }

    /// <inheritdoc />
    public TVal? GetBestValue<TVal>(string name, string?[] typeNames)
    {
        foreach (var type in typeNames)
        {
            var result = GetBestValue<TVal>(name, type);
            if (!EqualityComparer<TVal>.Default.Equals(result!, default!))
                return result;
        }
        return default;
    }
    #endregion

    #region Type Specific Data

    public bool HasType(string typeName)
        => this.Any(e => e.Type.Is(typeName));

    public IEnumerable<IEntity> OfType(string typeName)
        => MetadataWithoutPermissions.OfType(typeName);

    #endregion


    #region enumerators
    [PrivateApi]
    public IEnumerator<IEntity> GetEnumerator()
        => new EntityEnumerator(MetadataWithoutPermissions);

    [PrivateApi]
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    #endregion
}