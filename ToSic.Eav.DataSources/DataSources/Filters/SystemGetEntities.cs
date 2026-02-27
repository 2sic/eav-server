using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.DataSource.Sys;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that filters Entities by Ids. Can handle multiple IDs if comma-separated.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "System Get Entities",
    UiHint = "Find items based on one or more IDs",
    Icon = DataSourceIcons.Fingerprint,
    Type = DataSourceType.Filter, 
    NameId = "1012a387-c7db-4fa2-83c0-c1862bf0aadf",
    NameIds = ["System.GetEntities"],
    In = [InStreamDefaultRequired],
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal
)]
public class SystemGetEntities : DataSourceBase
{
    #region Configuration-properties

    /// <summary>
    /// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
    /// </summary>
    [Configuration]
    public string EntityIds => Configuration.GetThis<string>(fallback: "");

    #endregion

    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public SystemGetEntities(Dependencies services, IAppReaderFactory appReaders)
        : base(services, $"{DataSourceConstantsInternal.LogPrefix}.EntIdF", connect: [appReaders])
    {
        ProvideOut(() => GetList(appReaders));
    }

    private IImmutableList<IEntity> GetList(IAppReaderFactory appReaders)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var entityIdsOrError = this.CustomConfigurationParse(EntityIds);
        if (!entityIdsOrError.IsOk)
            return l.ReturnAsError(entityIdsOrError.ErrorsSafe());

        var entityIds = entityIdsOrError.Result!;

        var appReader = appReaders.Get(this.PureIdentity());
        if (appReader == null! /* paranoid */)
            return l.ReturnAsError(Error.TryGetInFailed());

        var result = entityIds
            .Select(eid => appReader.List.GetOne(eid)!)
            .Where(e => e != null!)
            .ToImmutableOpt();

        l.A(l.Try(() => $"get ids:[{string.Join(",", entityIds)}] found:{result.Count}"));
        return l.ReturnAsOk(result);
    }

}
