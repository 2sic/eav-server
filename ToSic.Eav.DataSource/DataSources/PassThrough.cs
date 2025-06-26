namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that forwards all `In` Connections. It's more for internal use.
/// </summary>
[PublicApi]

[VisualQuery(
    NiceName = "Pass-Through",
    UiHint = "Technical DataSource, doesn't do anything",
    Icon = DataSourceIcons.CopyAll,
    Type = DataSourceType.Source, 
    Audience = Audience.Advanced,
    NameId = "ToSic.Eav.DataSources.PassThrough, ToSic.Eav.DataSources",
    DynamicOut = true,
    OutMode = VisualQueryAttribute.OutModeMirrorIn, // New v20 - improved visual query
    DynamicIn = true)]
public class PassThrough : DataSourceBase
{
    /// <inheritdoc />
    /// <summary>
    /// Constructs a new PassThrough DataSources
    /// </summary>
    [PrivateApi]
    public PassThrough(MyServices services) : this(services, $"{DataSourceConstantsInternal.LogPrefix}.PasThr")
    {
    }

    [PrivateApi]
    protected PassThrough(MyServices services, string logName) : base(services, logName)
    {
    }

    /// <summary>
    /// The Out is the same as the In.
    /// </summary>
    public override IReadOnlyDictionary<string, IDataStream> Out => In;

    /// <summary>
    /// provide a static cachekey - as there is nothing dynamic on this source to modify the cache
    /// </summary>
    /// <remarks>
    /// if the key is not static (like the default setup) it will always cause errors
    /// </remarks>
    [PrivateApi]
    public override string CachePartialKey => "PassThrough";
}