using System.Collections.Immutable;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Metadata;
using ToSic.Lib.Coding;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Entities.Sys.Wrappers;

/// <summary>
/// This extends an existing <see cref="IEntity"/> with more properties and information. 
/// Everything in the original is passed through invisibly. <br/>
/// </summary>
[PrivateApi("this decorator object is for internal use only, no value in publishing it")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class EntityWrapper : IEntity, IEntityWrapper
{
    public IEntity Entity { get; }

    /// <summary>
    /// Initialize the object and store the underlying IEntity.
    /// </summary>
    /// <param name="baseEntity"></param>
    protected EntityWrapper(IEntity baseEntity)
    {
        Entity = baseEntity;
        RootContentsForEqualityCheck = Entity;

        // If it's not itself a wrapper, then we're done
        if (Entity is not IEntityWrapper wrapper)
            return;

        // If it's a wrapper, then we must track the root entity for equality checks
        RootContentsForEqualityCheck = wrapper.RootContentsForEqualityCheck ?? Entity;
        Decorators = [..wrapper.Decorators];
    }

    /// <summary>
    /// Initialize the object and store the underlying IEntity. - adding another decorator
    /// </summary>
    /// <param name="baseEntity"></param>
    /// <param name="decorator">Additional wrapper to add</param>
    public EntityWrapper(IEntity baseEntity, IDecorator<IEntity> decorator) : this(baseEntity)
    {
        if (decorator != null)
            Decorators = [..Decorators, decorator];
    }


    #region IEntity Implementation

    /// <inheritdoc />
    public int AppId => Entity.AppId;

    /// <inheritdoc />
    public int EntityId => Entity.EntityId;

    /// <inheritdoc />
    public int RepositoryId => Entity.RepositoryId;

    /// <inheritdoc />
    public Guid EntityGuid => Entity.EntityGuid;

    /// <inheritdoc />
    public ITarget MetadataFor => Entity.MetadataFor;

    /// <inheritdoc />
    public IImmutableDictionary<string, IAttribute> Attributes => Entity.Attributes;

    /// <inheritdoc />
    public IContentType Type => Entity.Type;

    /// <inheritdoc />
    public IAttribute Title => Entity.Title;

    /// <inheritdoc />
    public DateTime Modified => Entity.Modified;

    public DateTime Created => Entity.Created;

    /// <inheritdoc />
    public IAttribute this[string attributeName] => Entity[attributeName];

    /// <inheritdoc />
    public IEntityRelationships Relationships => Entity.Relationships;

    /// <inheritdoc />
    public bool IsPublished => Entity.IsPublished;

    /// <inheritdoc />
    public string Owner => Entity.Owner;

    /// <inheritdoc />
    public int OwnerId => Entity.OwnerId;

    /// <inheritdoc />
    [Obsolete("Should not be used anymore, use Get instead. planned to keep till ca. v20")]
    public object GetBestValue(string attributeName, string[] languages)
        => Entity.GetBestValue(attributeName, languages);

    [Obsolete("Should not be used anymore, use Get<T> instead. planned to keep till ca. v20")]
    public T GetBestValue<T>(string attributeName, string[] languages)
        => Entity.GetBestValue<T>(attributeName, languages);

    /// <inheritdoc />
    public string GetBestTitle()
        => Entity.GetBestTitle();

    /// <inheritdoc />
    public string GetBestTitle(string[] dimensions)
        => Entity.GetBestTitle(dimensions);

    /// <inheritdoc />
    public int Version => Entity.Version;

    /// <inheritdoc />
    public IMetadataOf Metadata => Entity.Metadata;

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Entity.Permissions;

    #endregion

    #region support for LINQ enhancements

    /// <inheritdoc />
    public List<IEntity> Children(string field = null, string type = null) => Entity.Children(field, type);

    /// <inheritdoc />
    public List<IEntity> Parents(string type = null, string field = null) => Entity.Parents(type, field);

    #endregion

    /// <inheritdoc />
    public object Value(string field) => Entity.Value(field);

    /// <inheritdoc />
    public T Value<T>(string field) => Entity.Value<T>(field);

    /// <inheritdoc />
    public object Get(string name) => Entity.Get(name);

    /// <inheritdoc />
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public object Get(string name, NoParamOrder noParamOrder = default, string language = default,
        string[] languages = default) =>
        Entity.Get(name, noParamOrder, language, languages);

    /// <inheritdoc />
    public TValue Get<TValue>(string name) => Entity.Get<TValue>(name);

    /// <inheritdoc />
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public TValue Get<TValue>(string name, NoParamOrder noParamOrder = default, TValue fallback = default,
        string language = default, string[] languages = default) =>
        Entity.Get(name, noParamOrder, fallback, language, languages);


    [PrivateApi("Internal")]
    public virtual PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        => Entity.FindPropertyInternal(specs, path.Add("Wrap", specs.Field));

    // #DropUseOfDumpProperties
    //[PrivateApi("Internal")]
    //public List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path) 
    //    => Entity._DumpNameWipDroppingMostCases(specs, path);

    public IEnumerable<IDecorator<IEntity>> Decorators { get; } = [];
}