using ToSic.Eav.Data.Sys.PropertyLookup;
using ToSic.Eav.Metadata;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Data.Sys.Entities;

/// <summary>
/// This extends an existing <see cref="IEntity"/> with more properties and information. 
/// Everything in the original is passed through invisibly. <br/>
/// </summary>
[PrivateApi("this decorator object is for internal use only, no value in publishing it")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class EntityWrapper : IEntity, IEntityWrapper
{
    public IEntity Entity { get; private set; }

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
    public EntityWrapper(IEntity baseEntity, IDecorator<IEntity>? decorator) : this(baseEntity)
    {
        if (decorator != null)
            Decorators = [..Decorators, decorator];
    }

    public bool SetupModel(IEntity? source)
    {
        Entity = source;
        RootContentsForEqualityCheck = Entity;

        // If it's not itself a wrapper, then we're done
        if (Entity is not IEntityWrapper wrapper)
            return true;

        // If it's a wrapper, then we must track the root entity for equality checks
        RootContentsForEqualityCheck = wrapper.RootContentsForEqualityCheck ?? Entity;
        Decorators = [..wrapper.Decorators];
        return true;
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
    public IReadOnlyDictionary<string, IAttribute> Attributes => Entity.Attributes;

    /// <inheritdoc />
    public IContentType Type => Entity.Type;

    /// <inheritdoc />
    public IAttribute? Title => Entity.Title;

    /// <inheritdoc />
    public DateTime Modified => Entity.Modified;

    public DateTime Created => Entity.Created;

    /// <inheritdoc />
    public IAttribute? this[string attributeName] => Entity[attributeName];

    /// <inheritdoc />
    public IEntityRelationships Relationships => Entity.Relationships;

    /// <inheritdoc />
    public bool IsPublished => Entity.IsPublished;

    /// <inheritdoc />
    public string Owner => Entity.Owner;

    /// <inheritdoc />
    public int OwnerId => Entity.OwnerId;
    
    /// <inheritdoc />
    public string? GetBestTitle()
        => Entity.GetBestTitle();

    /// <inheritdoc />
    public string? GetBestTitle(string?[] dimensions)
        => Entity.GetBestTitle(dimensions);

    /// <inheritdoc />
    public int Version => Entity.Version;

    /// <inheritdoc />
    public IMetadata Metadata => Entity.Metadata;

    /// <inheritdoc />
    public IEnumerable<IPermission> Permissions => Entity.Permissions;

    #endregion

    #region support for LINQ enhancements

    /// <inheritdoc />
    public IEnumerable<IEntity?> Children(string? field = null, string? type = null)
        => Entity.Children(field, type);

    /// <inheritdoc />
    public IEnumerable<IEntity> Parents(string? type = null, string? field = null)
        => Entity.Parents(type, field);

    #endregion

    /// <inheritdoc />
    public object? Get(string name) => Entity.Get(name);

    /// <inheritdoc />
    // ReSharper disable once MethodOverloadWithOptionalParameter
    public object? Get(string name, NoParamOrder npo = default, string? language = default, string?[]? languages = default)
        => Entity.Get(name, npo, language, languages);

    [PrivateApi("Internal")]
    public virtual PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        => Entity.FindPropertyInternal(specs, path.Add("Wrap", specs.Field));

    public IEnumerable<IDecorator<IEntity>> Decorators { get; private set; } = [];

}