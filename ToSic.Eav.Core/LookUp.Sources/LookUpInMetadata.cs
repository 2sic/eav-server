using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.LookUp.Sources;

/// <summary>
/// LookUp things from metadata. This uses EAV Metadata system and will look up Metadata for something.
/// As of now it's hardwired to look up Metadata of Entities. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[PrivateApi("used to be public, but it's not clear why anybody should use it. ATM not used in our code base - was only used for Queries, and that has a renamed copy LookUpInQueryMetadata")]
public class LookUpInMetadata : LookUpInEntity
{
    private readonly IMetadataSource _metaDataSource;
    private readonly Guid _objectToProvideSettingsTo;
    private readonly IEntity _parent;

    ///// <summary>
    ///// Constructs the object with pre-filled parameters. It won't access the entity yet, because 
    ///// it's possible that the data-source wouldn't be ready yet. The access to the entity will 
    ///// only occur if it's really needed. 
    ///// </summary>
    ///// <param name="name">Name of the PropertyAccess, e.g. PipelineSettings</param>
    ///// <param name="objectId">EntityGuid of the Entity to get assigned Entities of</param>
    ///// <param name="metaDataSource">DataSource that provides MetaData</param>
    //[Obsolete("Should not be used, as it doesn't have dimensions included.")]
    //public LookUpInMetadata(string name, Guid objectId, IMetadataSource metaDataSource) : base(name, null, null)
    //{
    //    _objectToProvideSettingsTo = objectId;
    //    _metaDataSource = metaDataSource;
    //}

    /// <summary>
    /// Alternate constructor where the entity with attached metadata is already known.
    /// The attached metadata will be used as source for the look-up
    /// </summary>
    /// <param name="name">Source name</param>
    /// <param name="entityWithMetadata">Entity whose metadata we'll use</param>
    /// <param name="dimensions">language / dimension data for lookup</param>
    public LookUpInMetadata(string name, IEntity entityWithMetadata, string[] dimensions): base(name, null, dimensions, "LookUp in Metadata")
    {
        _parent = entityWithMetadata;
    }

    /// <summary>
    /// For late-loading the entity. Will be called automatically by the Get if not loaded yet. 
    /// </summary>
    [PrivateApi]
    public void Initialize()
    {
        if (_initialized) return;
        var md = _parent?.Metadata ??
                 _metaDataSource.GetMetadata(TargetTypes.Entity, _objectToProvideSettingsTo);

        // make sure we get the settings, but not the pipeline-parts, which may also be assigned
        SetData(md.FirstOrDefault());
        _initialized = true;
    }
    private bool _initialized;


    /// <inheritdoc/>
    public override string Get(string key, string format)
    {
        Initialize();
        return base.Get(key, format);
    }
}