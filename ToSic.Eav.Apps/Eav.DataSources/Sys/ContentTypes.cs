using ToSic.Eav.Apps;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Repositories;
using ToSic.Sys.Utils;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that all content-types of an app.
/// </summary>
/// <remarks>
/// * New in v11.20
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Content Types",
    UiHint = "Types of an App",
    Icon = DataSourceIcons.Dns,
    Type = DataSourceType.System,
    NameId = "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
    Audience = Audience.Advanced,
    DynamicOut = false,
    ConfigurationType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
    NameIds =
    [
        "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
        // not sure if this was ever used...just added it for safety for now
        // can probably remove again, if we see that all system queries use the correct name
        "ToSic.Eav.DataSources.ContentTypes, ToSic.Eav.Apps"
    ],
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypes")]
// ReSharper disable once UnusedMember.Global
public sealed class ContentTypes: CustomDataSource
{
    #region Configuration-properties (no config)

    /// <summary>
    /// The app id
    /// </summary>
    [Configuration(Field = "AppId")]    // Legacy field name
    public int OfAppId => Configuration.GetThis(AppId);

    /// <summary>
    /// The scope to get the content types of - normally it's only the default scope
    /// </summary>
    /// <remarks>
    /// * Renamed to `Scope` in v15, previously was called `OfScope`
    /// </remarks>
    [Configuration(Fallback = "Default")]
    public string Scope => Configuration.GetThis();

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new ContentTypes DS
    /// </summary>
    [PrivateApi]
    public ContentTypes(MyServices services, IAppReaderFactory appReaders): base(services, $"{DataSourceConstantsInternal.LogPrefix}.CTypes")
    {
        ConnectLogs([_appReaders = appReaders]);
        ProvideOut(GetList, options: () => ContentTypeUtil.Options with { AppId = OfAppId, WithMetadata = true });
    }
    private readonly IAppReaderFactory _appReaders;

    private IEnumerable<IRawEntity> GetList()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();

        var appId = OfAppId;
        var scp = Scope.UseFallbackIfNoValue(Data.Scopes.Default);

        var types = _appReaders.Get(appId).ContentTypes.OfScope(scp, includeAttributeTypes: true);

        // Deduplicate, in case we have identical types on current app and inherited
        var deDuplicate = types
            .GroupBy(t => t.NameId)
            .Select(g =>
            {
                // Just 1
                if (g.Count() == 1) return g.First();

                // More than 1, prioritize of the current app before parent-apps; SQL before File-System
                var ofCurrentApp = g.Where(t => t.AppId == appId).ToList();
                if (ofCurrentApp.Any())
                    return ofCurrentApp.FirstOrDefault(t => t.RepositoryType == RepositoryTypes.Sql)
                           ?? ofCurrentApp.First();

                // Fallback: just return 1
                return g.First();
            })
            .ToList();

        var list = deDuplicate
            .OrderBy(t => t.Name)
            .Select(ContentTypeUtil.ToRaw)
            .ToList();


        return l.Return(list, $"{list.Count}");
    }
}