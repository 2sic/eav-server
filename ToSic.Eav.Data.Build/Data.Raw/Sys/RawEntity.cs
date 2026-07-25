using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// A ready-to-use <see cref="IRawEntity"/> which receives all the data in the constructor.
///
/// Use this for scenarios where you don't want to create your own IRawEntity but wish to return this kind of typed object.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RawEntity: IRawEntity, IRelationshipKeys, IHasMetadata
{
    #region Constructor Variants
    
    public RawEntity()
    { }

    public RawEntity(Dictionary<string, object?> values)
    {
        Values = values;
    }

    #endregion


    #region Core Properties Id, Guid, Created, Modified

    public virtual int Id
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    public virtual Guid Guid
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = Guid.Empty;

    public virtual DateTime Created
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    public virtual DateTime Modified
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    #endregion


    #region Values

    public IDictionary<string, object?> Values
    {
        get => field ??= GetValues();
        init => field = value?.ToInvariant();
    }

    /// <summary>
    /// Override this method to provide the values for the entity. By default, it returns an empty dictionary.
    /// </summary>
    /// <returns></returns>
    protected virtual IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);

    #endregion


    public virtual IEnumerable<object> RelationshipKeys
    {
        get => [];
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// WIP experimental v18.02 - trying to get content-type metadata into the raw entity
    /// </summary>
    public IMetadata? Metadata { get; init; }
}