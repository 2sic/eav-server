using ToSic.Eav.Apps;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSource.Sys;

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
    NameId = "d5372be5-3b29-45dd-9b74-97408bba2d42",
    Audience = Audience.Advanced,
    ConfigurationType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
    NameIds =
    [
        "System.ContentTypes",
        "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
        // not sure if this was ever used...just added it for safety for now
        // can probably remove again, if we see that all system queries use the correct name
        // 2025-12-02 removed v20.00-09
        //"ToSic.Eav.DataSources.ContentTypes, ToSic.Eav.Apps"
    ],
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypes")]
// ReSharper disable once UnusedMember.Global
public sealed class ContentTypes: CustomDataSource
{
    #region Configuration-properties

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
    public string Scope => Configuration.GetThis(fallback: "Default");

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new ContentTypes DS
    /// </summary>
    [PrivateApi]
    public ContentTypes(Dependencies services, IAppReaderFactory appReaders)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.CTypes", connect: [appReaders])
    {
        _appReaders = appReaders;
        ProvideOutRaw(GetList, options: () => new()
        {
            AppId = OfAppId,
            WithMetadata = true,
            AllowUnknownValueTypes = true
        });
    }
    private readonly IAppReaderFactory _appReaders;

    private IEnumerable<ContentTypeUtil.ContentTypeSummary> GetList()
    {
        var l = Log.Fn<IEnumerable<ContentTypeUtil.ContentTypeSummary>>();

        var appId = OfAppId;
        // Get the scope. Make sure that an empty string will be ignored and "Default" is used
        var scope = Scope.UseFallbackIfNoValue(ScopeConstants.Default);

        var appReader = _appReaders.Get(appId);
        var types = appReader.ContentTypes.OfScope(scope, includeAttributeTypes: true);

        // Deduplicate, in case we have identical types on current app and inherited
        var deDuplicate = types
            .GroupBy(type => type.NameId)
            .Select(group =>
            {
                // Just 1
                if (group.Count() == 1)
                    return group.First();

                // More than 1, prioritize of the current app before parent-apps; SQL before File-System
                var ofCurrentApp = group
                    .Where(type => type.AppId == appId)
                    .ToList();
                if (ofCurrentApp.Any())
                    return ofCurrentApp
                               .FirstOrDefault(type => type.RepositoryType == RepositoryTypes.Sql)
                           ?? ofCurrentApp.First();

                // Fallback: just return 1
                return group.First();
            })
            .ToList();

        var itemCounts = appReader.List
            .GroupBy(entity => entity.Type.NameId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entity => entity.EntityId).Distinct().Count());

        var list = deDuplicate
            .OrderBy(type => type.Name)
            .Select(type => new ContentTypeUtil.ContentTypeSummary(
                type,
                itemCounts.TryGetValue(type.NameId, out var itemCount) ? itemCount : 0))
            .ToList();

        return l.Return(list, $"{list.Count}");
    }
}