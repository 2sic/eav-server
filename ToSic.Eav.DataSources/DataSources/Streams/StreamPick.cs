using ToSic.Eav.Plumbing;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that returns a stream by the provided name.
/// Usually this will be configured through [Params:SomeName]
/// </summary>
/// <remarks>Introduced in 10.26</remarks>
[PublicApi]
[VisualQuery(
    NiceName = "Pick Stream",
    UiHint = "Choose a stream",
    Icon = DataSourceIcons.Merge,
    Type = DataSourceType.Logic,
    NameId = "ToSic.Eav.DataSources.StreamPick, ToSic.Eav.DataSources",
    ConfigurationType = "67b19864-df6d-400b-9f37-f41f1dd69c4a",
    DynamicOut = false,
    DynamicIn = true,
    HelpLink = "https://go.2sxc.org/DsStreamPick")]

public sealed class StreamPick : DataSourceBase
{
    #region Configuration-properties


    /// <summary>
    /// The stream name to lookup.
    /// </summary>
    [Configuration(Fallback = DataSourceConstants.StreamDefaultName)]
    public string StreamName
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    ///// <summary>
    ///// The attribute whose value will be sorted by.
    ///// </summary>
    ///// <remarks>
    ///// This feature has not been fully implemented yet. The idea would be that it could access an "App" or similar and dynamically access streams from there
    ///// This may also have a security risk, so don't finish till this is clarified
    ///// </remarks>
    //public bool UseParentStreams
    //{
    //    get => bool.TryParse(Configuration[SearchInParentKey], out var result) && result;
    //    set => Configuration.SetThis(value);
    //}
    //private const string SearchInParentKey = "UseParentStreams";

    #endregion


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public StreamPick(MyServices services) : base(services, $"{DataSourceConstants.LogPrefix}.StmPck")
    {
        ProvideOut(StreamPickList);
    }

    private IImmutableList<IEntity> StreamPickList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();
        var name = StreamName;
        l.A($"StreamName to Look for: '{name}'");
        if (string.IsNullOrWhiteSpace(StreamName))
            return l.ReturnAsError(ImmutableList<IEntity>.Empty, "no name");

        var foundStream = In.FirstOrDefault(pair => pair.Key.EqualsInsensitive(name));

        if (!string.IsNullOrEmpty(foundStream.Key))
            return l.ReturnAsOk(foundStream.Value.List.ToImmutableList());

        // Error not found
        var msg = $"StreamPick can't find stream by the name '{StreamName}'";
        l.A(msg);
        return l.ReturnAsError(Error.Create(title: "Can't find Stream", message: $"Trying to pick the stream '{StreamName}' but it doesn't exist on the In."));
    }

}