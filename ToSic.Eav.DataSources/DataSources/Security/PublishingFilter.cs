using ToSic.Eav.DataSource.Internal.Query;
using ToSic.Sys.Users.Permissions;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Filter entities to show Drafts or only Published Entities.
///
/// It will decide based on the current users authorizations to deliver the inbound "draft" stream or the "published" stream
/// on the out stream.This is usually used when an App is its main input, as it provides these two streams for performance reasons.
///
/// Enhanced in v18 for scenarios where the data is not pre-split but comes as a normal stream.
/// In this case it will take the "Default" In-Stream and filter it based on the user's permissions. (beta)
/// </summary>
/// <remarks>
/// * Created ca. v07.00
/// * Enhanced v18.00 to also support filtering data on the "Default" in stream in addition to the original (beta)
/// </remarks>
[PublicApi]

[VisualQuery(
    NiceName = "Publishing Filter",
    UiHint = "Keep data based on user roles (editor sees draft items)",
    Icon = DataSourceIcons.Eye, 
    Type = DataSourceType.Security, 
    NameId = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
    In = [DataSourceConstantsInternal.StreamPublishedName + "*", StreamDefaultName + "*",  DataSourceConstantsInternal.StreamDraftsName + "*"],
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
    public PublishingFilter(MyServices services, ICurrentContextUserPermissionsService userPermissions) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Publsh", connect: [userPermissions])
    {
        _userPermissions = userPermissions;
        ProvideOut(PublishingFilterList);
    }
    private readonly ICurrentContextUserPermissionsService _userPermissions;


    private IImmutableList<IEntity> PublishingFilterList()
    {
        var showDraftsSetting = ShowDrafts;
        var l = Log.Fn<IImmutableList<IEntity>>();
        var finalShowDrafts = showDraftsSetting
                              ?? _userPermissions.UserPermissions()?.ShowDraftData
                              ?? QueryConstants.ParamsShowDraftsDefault;
        var inStreamName = finalShowDrafts
            ? DataSourceConstantsInternal.StreamDraftsName
            : DataSourceConstantsInternal.StreamPublishedName;

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