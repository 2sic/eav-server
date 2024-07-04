using ToSic.Eav.Context;
using ToSic.Eav.DataSource.Internal.Query;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Filter entities to show Drafts or only Published Entities
/// </summary>
[PublicApi]

[VisualQuery(
    NiceName = "Publishing Filter",
    UiHint = "Keep data based on user roles (editor sees draft items)",
    Icon = DataSourceIcons.Eye, 
    Type = DataSourceType.Security, 
    NameId = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
    In = [StreamPublishedName + "*", StreamDefaultName + "*",  StreamDraftsName + "*"],
    DynamicOut = false, 
    HelpLink = "https://go.2sxc.org/DsPublishingFilter")]

public class PublishingFilter : DataSourceBase
{

    #region Configuration-properties

    /// <summary>
    /// Indicates whether to show drafts or only Published Entities. 
    /// </summary>
    [Configuration(Fallback = null)]
    public bool? ShowDrafts
    {
        get => Configuration.GetThis<bool?>(null);
        set => Configuration.SetThisObsolete(value);
    }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new PublishingFilter
    /// </summary>
    [PrivateApi]
    public PublishingFilter(MyServices services, IContextResolverUserPermissions userPermissions) : base(services, $"{LogPrefix}.Publsh", connect: [userPermissions])
    {
        _userPermissions = userPermissions;
        ProvideOut(PublishingFilterList);
    }
    private readonly IContextResolverUserPermissions _userPermissions;


    private IImmutableList<IEntity> PublishingFilterList()
    {
        var showDraftsSetting = ShowDrafts;
        var l = Log.Fn<IImmutableList<IEntity>>();
        var finalShowDrafts = showDraftsSetting
                              ?? _userPermissions.UserPermissions()?.ShowDraftData
                              ?? QueryConstants.ParamsShowDraftsDefault;
        var inStreamName = finalShowDrafts
            ? StreamDraftsName
            : StreamPublishedName;

        // Standard / old case: if the inputs already have the correct streams, use them.
        if (In.TryGetValue(inStreamName, out var inStream))
        {
            var result = inStream.List.ToImmutableList();
            return l.Return(result, $"Show Draft setting:'{showDraftsSetting}'; final:{finalShowDrafts}; stream: {inStreamName}; count: {result.Count}");
        }

        if (In.TryGetValue(StreamDefaultName, out var inDefault))
        {
            var filtered = finalShowDrafts
                ? inDefault.List.ToImmutableList()
                : inDefault.List.Where(e => e.IsPublished).ToImmutableList();
            return l.Return(filtered, $"Refiltering the Default; setting:'{showDraftsSetting}'; final:{finalShowDrafts}; stream: {StreamDefaultName}; count: {filtered.Count}");
        }

        return l.ReturnAsError(Error.TryGetInFailed(name: $"{inStreamName}/{StreamDefaultName}"));
    }

}