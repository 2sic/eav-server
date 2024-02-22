using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;

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

    private const string AppIdKey = "AppId";
    private const string ContentTypeTypeName = "ContentType";

    /// <summary>
    /// The app id
    /// </summary>
    [Configuration(Field = AppIdKey)]
    public int OfAppId => Configuration.GetThis(AppId);

    /// <summary>
    /// The scope to get the content types of - normally it's only the default scope
    /// </summary>
    /// <remarks>
    /// * Renamed to `Scope` in v15, previously was called `OfScope`
    /// </remarks>
    [Configuration(Fallback = "Default")]
    public string Scope => Configuration.GetThis();

    [PrivateApi]
    [Obsolete("Do not use anymore, use Scope instead - only left in for compatibility. Probably remove v17 or something")]
    public string OfScope => Scope;

    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new ContentTypes DS
    /// </summary>
    [PrivateApi]
    public ContentTypes(MyServices services, IAppStates appStates): base(services, $"{DataSourceConstants.LogPrefix}.CTypes")
    {
        ConnectServices(
            _appStates = appStates
        );
        var options = new DataFactoryOptions(typeName: ContentTypeTypeName, titleField: ContentTypeType.Name.ToString());
        ProvideOut(GetList, options: () => new(options, appId: OfAppId));
    }
    private readonly IAppStates _appStates;

    private IEnumerable<IRawEntity> GetList()
    {
        var l = Log.Fn<IEnumerable<IRawEntity>>();
        Configuration.Parse();

        var appId = OfAppId;
        var scp = Scope.UseFallbackIfNoValue(Data.Scopes.Default);

        var types = _appStates.GetReader(appId).ContentTypes.OfScope(scp, includeAttributeTypes: true);

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
            .Select(t => new RawEntity(ContentTypeUtil.BuildDictionary(t))
            {
                Id = t.Id,
                Guid = SafeConvertGuid(t) ?? Guid.Empty
            })
            .ToList();


        return l.Return(list, $"{list.Count}");
    }

    private static Guid? SafeConvertGuid(IContentType t)
    {
        Guid? guid = null;
        try
        {
            if (Guid.TryParse(t.NameId, out var g)) guid = g;
        }
        catch
        {
            /* ignore */
        }

        return guid;
    }
}