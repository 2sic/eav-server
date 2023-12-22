using System.Collections.Immutable;
using ToSic.Eav.Context;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Filter entities to show Drafts or only Published Entities
/// </summary>
[PublicApi_Stable_ForUseInYourCode]

[VisualQuery(
    NiceName = "Publishing Filter",
    UiHint = "Keep data based on user roles (editor sees draft items)",
    Icon = Icons.Eye, 
    Type = DataSourceType.Security, 
    NameId = "ToSic.Eav.DataSources.PublishingFilter, ToSic.Eav.DataSources",
    In = new []{ StreamPublishedName + "*", StreamDefaultName + "*",  StreamDraftsName + "*" },
    DynamicOut = false, 
    HelpLink = "https://go.2sxc.org/DsPublishingFilter")]

public class PublishingFilter : Eav.DataSource.DataSourceBase
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
    public PublishingFilter(MyServices services, IContextResolverUserPermissions userPermissions) : base(services, $"{LogPrefix}.Publsh")
    {
        ConnectServices(
            _userPermissions = userPermissions
        );
        ProvideOut(PublishingFilterList);
    }
    private readonly IContextResolverUserPermissions _userPermissions;


    private IImmutableList<IEntity> PublishingFilterList()
    {
        Configuration.Parse();
        var showDraftsInSettings = ShowDrafts;
        var finalShowDrafts = ShowDrafts ?? _userPermissions.UserPermissions()?.UserMayEdit ?? QueryConstants.ParamsShowDraftsDefault;
        Log.A($"get incl. draft:'{showDraftsInSettings}' = '{finalShowDrafts}'");
        var outStreamName = finalShowDrafts ? StreamDraftsName : StreamPublishedName;
        return In[outStreamName].List.ToImmutableList();
    }

}