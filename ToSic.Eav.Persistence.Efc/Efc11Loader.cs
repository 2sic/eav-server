using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.Persistence.Efc;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class Efc11Loader(
    EavDbContext context,
    LazySvc<IZoneCultureResolver> environmentLazy,
    IAppInitializedChecker initializedChecker,
    IAppsCatalog appsCatalog,
    IAppStateCacheService appStates,
    ILogStore logStore,
    LazySvc<IEavFeaturesService> featuresService,
    DataBuilder dataBuilder,
    Generator<IDataDeserializer> dataDeserializer,
    Generator<IAppContentTypesLoader> appFileContentTypesLoader,
    Generator<IAppStateBuilder> appStateBuilder)
    : ServiceBase("Db.Efc11",
        connect:
        [
            context, environmentLazy, initializedChecker, appsCatalog, appStates, logStore, featuresService, dataBuilder,
            dataDeserializer, appFileContentTypesLoader, appStateBuilder
        ]), IRepositoryLoader
{

    public Efc11Loader UseExistingDb(EavDbContext dbContext)
    {
        context = dbContext;
        return this;
    }

    internal TimeSpan SqlTotalTime = new(0);

    internal void AddSqlTime(TimeSpan sqlTime) => SqlTotalTime = SqlTotalTime.Add(sqlTime);

    internal EavDbContext Context => context;

    public string PrimaryLanguage
    {
        get
        {
            if (_primaryLanguage != null) return _primaryLanguage;
            var l = Log.Fn<string>();
            _primaryLanguage = environmentLazy.Value.DefaultCultureCode.ToLowerInvariant();
            return l.ReturnAndLog(_primaryLanguage, $"Primary language from Env (for value sorting): {_primaryLanguage}");
        }
        set => _primaryLanguage = value;
    }
    private string _primaryLanguage;

    #region App Entities

    internal void LoadEntities(IAppStateBuilder builder, CodeRefTrail codeRefTrail, int[] entityIds = null)
    {
        var entityLoader = new EntityLoader(this, featuresService.Value, dataDeserializer, dataBuilder);
        var entitySqlTime = entityLoader.LoadEntities(builder, codeRefTrail, entityIds);
        AddSqlTime(entitySqlTime);
    }

    #endregion


    #region IRepositoryLoader Zones and ContentTypes

    IDictionary<int, Zone> IRepositoryLoader.Zones() => new ZoneLoader(this).Zones(logStore);

    /// <inheritdoc />
    /// <summary>
    /// Get all ContentTypes for specified AppId. 
    /// If uses temporary caching, so if called multiple times it loads from a private field.
    /// </summary>
    IList<IContentType> IContentTypeLoader.ContentTypes(int appId, IHasMetadataSourceAndExpiring source)
        => new ContentTypeLoader(this, appFileContentTypesLoader, dataDeserializer, dataBuilder, appStates)
            .LoadContentTypesFromDb(appId, source);

    #endregion

    #region AppPackage Loader

    private AppLoader AppLoader => _appLoader ??= new(this, logStore, appsCatalog, featuresService.Value, initializedChecker, appStates, appStateBuilder, appFileContentTypesLoader, dataDeserializer, dataBuilder);
    private AppLoader _appLoader;


    public IAppStateBuilder AppStateBuilderRaw(int appId, CodeRefTrail codeRefTrail)
        => AppLoader.AppStateBuilderRaw(appId, codeRefTrail);

    /// <inheritdoc />
    public IAppStateCache AppStateInitialized(int appId, CodeRefTrail codeRefTrail)
        => AppLoader.AppStateInitialized(appId, codeRefTrail);

    public IAppStateCache Update(IAppStateCache appStateOriginal, AppStateLoadSequence startAt, CodeRefTrail codeRefTrail, int[] entityIds = null)
        => AppLoader.Update(appStateOriginal, startAt, codeRefTrail, entityIds);

    #endregion

}