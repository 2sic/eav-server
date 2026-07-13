namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// new, not in use yet
/// </summary>
/// <remarks>
/// wip v22
/// </remarks>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record RawEntityRecordBase : IRawEntity, IHasRelationshipKeys
{
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

    public abstract IDictionary<string, object?> Values { get; }

    public virtual IEnumerable<object> RelationshipKeys => [];
}