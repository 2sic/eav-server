using ToSic.Eav.Apps;
using ToSic.Eav.LookUp.Sources;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.DataSource.Query.Sys;

/// <summary>
/// Carries the caller's resolved permissions through a query without changing scoped context state.
/// </summary>
[PrivateApi]
public sealed class QueryUserPermissions(IAppIdentity app, EffectivePermissions permissions)
    : LookUpInDictionary(SourceName)
{
    private const string SourceName = "QueryUserPermissions";
    private readonly int _appId = app.AppId;
    private readonly int _zoneId = app.ZoneId;

    /// <summary>
    /// Returns the query's draft decision for its app, or null to keep the existing permission-service fallback.
    /// A different target app must not inherit this query's app-specific permissions.
    /// </summary>
    public static bool? ShowDrafts(ILookUpEngine? lookUp, IAppIdentity app)
        => lookUp?.FindSource(SourceName) is QueryUserPermissions source
           && source._appId == app.AppId && source._zoneId == app.ZoneId
            ? source.Permissions.ShowDraftData
            : null;

    private EffectivePermissions Permissions { get; } = permissions;
}
