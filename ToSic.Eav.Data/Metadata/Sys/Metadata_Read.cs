using System.Collections;
using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Lib.Coding;

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
    #region Get

    /// <inheritdoc />
    public TVal? Get<TVal>(string name)
        => GetMaxOneType<TVal>(name, typeName: null);

    /// <inheritdoc />
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public TVal? Get<TVal>(string name, NoParamOrder noParamOrder = default, string? typeName = null, IEnumerable<string?>? typeNames = default)
    {
        // Check if multiple type names were specified
        var typeNameList = typeNames?.ToListOpt();
        if (typeNameList.SafeAny())
            return GetManyTypes<TVal>(name, typeNameList);

        // If not, just use the single type name; could be null.
        return GetMaxOneType<TVal>(name, typeName);
    }

    private TVal? GetMaxOneType<TVal>(string name, string? typeName = null)
    {
        var list = typeName == null
            ? MetadataWithoutPermissions
            : OfType(typeName);
        var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
        return found == null
            ? default
            : found.Get<TVal>(name);
    }

    private TVal? GetManyTypes<TVal>(string name, IList<string?> typeNames)
    {
        foreach (var type in typeNames)
        {
            var result = Get<TVal>(name, typeName: type);
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