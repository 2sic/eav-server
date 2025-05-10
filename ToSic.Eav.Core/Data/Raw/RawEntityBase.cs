namespace ToSic.Eav.Data.Raw;

/// <summary>
/// Base class for raw entities which makes implementations a bit simpler when not much is needed.
/// For example, the dates default to now.
///
/// You can inherit this class, but you can also just re-implement the interface yourself.
/// Whatever works better for you.
/// </summary>
/// <remarks>
/// Added in 15.04
/// </remarks>
[PrivateApi("was public till 16.09")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class RawEntityBase: IRawEntity, IHasRelationshipKeys
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

    public abstract IDictionary<string, object> Attributes(RawConvertOptions options);

    public virtual IEnumerable<object> RelationshipKeys(RawConvertOptions options) => new List<object>();
}