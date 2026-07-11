namespace ToSic.Eav.Data.Raw.Sys;

/// <summary>
/// Record to provide a raw entity. It should never be inherited, as it's purpose is to bridge the gap between
/// PoCos and raw entity converters.
/// </summary>
/// <remarks>
/// Added in v22
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record RawEntityRecord: IRawEntity
{
    public int Id
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    public Guid Guid
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = Guid.Empty;

    public DateTime Created
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    public DateTime Modified
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = DateTime.Now;

    public required IDictionary<string, object?> AttributesWip { get; init; }

    public IDictionary<string, object?> Attributes(RawConvertOptions options) => AttributesWip;

    IConvertToRawEntity IGetRawConverter.GetConverter() => ConvertToRawSelf.Instance;
}