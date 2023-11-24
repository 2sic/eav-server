using System.Linq;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.LookUp;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.LookUp;

/// <summary>
/// LookUp things from metadata. This uses EAV Metadata system and will look up Metadata for something.
/// As of now it's hardwired to look up Metadata of Entities. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[PrivateApi]
internal class LookUpInQueryMetadata : LookUpInEntity
{
    private readonly IEntity _parent;

    /// <summary>
    /// Alternate constructor where the entity with attached metadata is already known.
    /// The attached metadata will be used as source for the look-up
    /// </summary>
    /// <param name="name">Source name</param>
    /// <param name="entityWithMetadata">Entity whose metadata we'll use</param>
    /// <param name="dimensions">language / dimension data for lookup</param>
    public LookUpInQueryMetadata(string name, IEntity entityWithMetadata, string[] dimensions): base(name, null, dimensions)
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
        // make sure we get the settings, but not the pipeline-parts, which may also be assigned
        Data = _parent.Metadata.FirstOrDefault(e => e.Type.NameId != QueryConstants.QueryPartTypeName);
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