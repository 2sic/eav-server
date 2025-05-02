using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Parts;
using ToSic.Eav.StartUp;

namespace ToSic.Eav.Repository.Efc;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DbDataController(
    EavDbContext dbContext,
    LazySvc<EfcAppLoader> efcLoaderLazy,
    LazySvc<IUser> userLazy,
    AppsCacheSwitch appsCache,
    Generator<JsonSerializer> jsonSerializerGenerator,
    ILogStore logStore,
    LazySvc<Compressor> compressor,
    DataBuilder builder)
    : ServiceBase("Db.Data",
        connect: [efcLoaderLazy, userLazy, appsCache, logStore, dbContext, jsonSerializerGenerator, compressor, builder]
    ), IStorage, IAppIdentity
{

    #region Properties like AppId, ZoneId, UserName etc.

    /// <summary>
    /// AppId of this whole Context
    /// </summary>
    public int AppId => _appId == Constants.AppIdEmpty ? Constants.MetaDataAppId : _appId;
    private int _appId;

    /// <summary>
    /// AppIds with parent AppId if it's set.
    /// Will only have another ID if a non-default parent is used, e.g. in inherited apps.
    /// </summary>
    public int[] AppIds => ParentAppId == null ? [AppId] : [AppId, ParentAppId.Value];

    /// <summary>
    /// ZoneId of this whole Context
    /// </summary>
    public int ZoneId => _zoneId == 0 ? Constants.DefaultZoneId : _zoneId;
    private int _zoneId;

    private const string UserNameUnknown = "unresolved(eav)";

    /// <summary>
    /// Current UserName. Used for TransactionId
    /// </summary>
    public string UserIdentityToken
    {
        get
        {
            if (field != null) return field;
            try
            {
                // try to get using dependency injection
                return field = userLazy.Value?.IdentityToken ?? UserNameUnknown;
            }
            catch
            {
                return field = UserNameUnknown;
            }
        }
    }

    /// <summary>
    /// ParentAppId of this whole Context, for scenarios where we have inherited apps.
    /// </summary>
    public int? ParentAppId { get; private set; }

    #endregion

    #region Parts

    internal DbVersioning Versioning => field ??= new(this, compressor);
    internal DbEntity Entities => field ??= new(this, builder);
    internal DbValue Values => field ??= new(this);
    internal DbAttribute Attributes => field ??= new(this);
    internal DbRelationship Relationships => field ??= new(this);
    internal DbAttributeSet AttribSet => field ??= new(this);
    internal DbPublishing Publishing => field ??= new(this, builder);
    internal DbDimensions Dimensions => field ??= new(this);
    internal DbZone Zone => field ??= new(this);
    internal DbApp App => field ??= new(this);
    internal DbContentType ContentType => field ??= new(this);

    #endregion


    public List<LogItem> ImportLogToBeRefactored { get; } = [];


    #region Constructor and Init

    private static bool _loggedToBootLog;

    public EavDbContext SqlDb
    {
        get
        {
            if (field != null)
                return field;

            if (!_loggedToBootLog)
            {
                BootLog.Log.A("DbDataController DB accessed");
                _loggedToBootLog = true;
            }

            // When used the first time, make sure we have the save handle attached
            dbContext.AlternateSaveHandler += SaveChanges;
            return field = dbContext;
        }
    }

    internal Generator<JsonSerializer> JsonSerializerGenerator { get; } = jsonSerializerGenerator;

    /// <summary>
    /// Set ZoneId, AppId and ParentAppId on current context.
    /// </summary>
    /// <param name="appState"></param>
    /// <returns></returns>
    public DbDataController Init(IAppReader appState)
        => Init(appState.ZoneId, appState.AppId, appState.GetParentCache()?.AppId);

    /// <summary>
    /// Set ZoneId and AppId on current context.
    /// </summary>
    public DbDataController Init(int? zoneId, int? appId, int? parentAppId = default)
    {
        // No matter what scenario we have, always set the parent app id - if given
        // todo: maybe later also try to detect it, if we see the need for it
        ParentAppId = parentAppId;

        // If we don't have a zoneId, ensure we set everything from defaults or lookup
        if (!zoneId.HasValue)
            // If nothing is supplied, use defaults
            if (!appId.HasValue)
            {
                _zoneId = Constants.DefaultZoneId;
                _appId = Constants.MetaDataAppId;
                return this;
            }
            // If only AppId is supplied, look up it's zone and use that
            else
            {
                var zoneIdOfApp = SqlDb.ToSicEavApps.Where(a => a.AppId == appId.Value).Select(a => (int?)a.ZoneId)
                    .SingleOrDefault();
                if (!zoneIdOfApp.HasValue)
                    throw new ArgumentException($@"App with id {appId.Value} doesn't exist.", nameof(appId));
                _appId = appId.Value;
                _zoneId = zoneIdOfApp.Value;
                return this;
            }

        // if only ZoneId was supplied, use that...
        _zoneId = zoneId.Value;

        if (appId.HasValue)
        {
            var foundApp = SqlDb.ToSicEavApps.FirstOrDefault(a => a.ZoneId == _zoneId && a.AppId == appId.Value);
            if (foundApp == null)
                throw new ArgumentException($@"App with id {appId.Value} doesn't exist.", nameof(appId));
            _appId = appId.Value;
        }
        else
            _appId = SqlDb.ToSicEavApps.First(a => a.Name == Constants.DefaultAppGuid).AppId;

        return this;
    }

    #endregion

    #region Save and check if to kill cache

    /// <summary>
    /// Persists all updates to the data source and optionally resets change tracking in the object context.
    /// Also Creates an initial TransactionId (used by SQL Server for Auditing).
    /// If items were modified, Cache is purged on current Zone/App
    /// </summary>
    public int SaveChanges(bool acceptAllChangesOnSuccess, EavDbContext.SaveChangesEvent baseEvent)
    {
        if (_appId == Constants.AppIdEmpty)
            throw new("SaveChanges with AppId 0 not allowed.");

        Versioning.GetTransactionId();

        var modifiedCount = baseEvent(acceptAllChangesOnSuccess);

        if (modifiedCount != 0)
            PurgeAppCacheIfReady();

        return modifiedCount;
    }

    private void PurgeAppCacheIfReady()
    {
        var l = Log.Fn($"{_purgeAppCacheOnSave}");
        if (_purgeAppCacheOnSave)
            appsCache.Value.Purge(this);
        l.Done();
    }

    #endregion

    #region Shorthand for do & save

    internal void DoAndSave(Action action, string message = null)
    {
        var l = Log.Fn(message: message, timer: true);
        action.Invoke();
        SqlDb.SaveChanges();
        l.Done();
    }


    internal void DoAndSaveWithoutChangeDetection(Action action, string message = null)
    {
        var l = Log.Fn(timer: true, message: message);
        action.Invoke();

        var preserve = SqlDb.ChangeTracker.AutoDetectChangesEnabled;
        try
        {
            SqlDb.ChangeTracker.AutoDetectChangesEnabled = false;
            SqlDb.SaveChanges();
        }
        catch (Exception ex)
        {
            l.A($"error: save changes without change detection failed, {ex.Message}");
            l.Ex(ex);
        }
        finally
        {
            SqlDb.ChangeTracker.AutoDetectChangesEnabled = preserve;
            l.Done();
        }
    }


    public void DoInTransaction(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        var ownTransaction = SqlDb.Database.CurrentTransaction == null
            ? SqlDb.Database.BeginTransaction()
            : null;

        var l = Log.Fn(timer: true, message: $"id:{randomId} - create new trans:{ownTransaction != null}");
        {
            try
            {
                action.Invoke();
                ownTransaction?.Commit();
                l.A($"Transaction {randomId} - completed"); // adds ok to end of block
                l.Done("transaction ok");   // adds ok to top of block
            }
            catch (Exception e)
            {
                ownTransaction?.Rollback();
                l.A($"Transaction {randomId} failed / rollback");
                l.A("transaction failed / rollback");
                l.Ex(e);
                throw;
            }
        }
    }

    /// <summary>
    /// Get or seth whether SaveChanges() should automatically purge cache.
    /// </summary>
    /// <remarks>Useful if many changes are made in a batch and Cache should be purged after that batch</remarks>
    private bool _purgeAppCacheOnSave = true;

    public void DoButSkipAppCachePurge(Action action)
    {
        var l = Log.Fn(timer: true);
        var before = _purgeAppCacheOnSave;
        _purgeAppCacheOnSave = false;
        action.Invoke();
        _purgeAppCacheOnSave = before;
        l.Done();
    }

    public void DoWithDelayedCacheInvalidation(Action action)
    {
        var l = Log.Fn(timer: true);
        _purgeAppCacheOnSave = false;
        action.Invoke();

        _purgeAppCacheOnSave = true;
        PurgeAppCacheIfReady();
        l.Done();
    }

    /// <summary>
    /// The loader must use the same connection, to ensure it runs in existing transactions.
    /// Otherwise, the loader would be blocked from getting intermediate data while we're running changes. 
    /// </summary>
    public IRepositoryLoader Loader => field ??= efcLoaderLazy.Value.UseExistingDb(SqlDb);

    public void DoWhileQueuingVersioning(Action action)
        => Versioning.DoAndSaveHistoryQueue(action);

    public void DoWhileQueueingRelationships(Action action)
        => Relationships.DoWhileQueueingRelationships(action);

    /// <summary>
    /// Save a list of entities together in a transaction.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="saveOptions">never null!</param>
    /// <returns></returns>
    public List<int> Save(List<IEntity> entities, SaveOptions saveOptions)
    {
        var pairs = entities
            .Select(IEntityPair<SaveOptions> (e) => new EntityPair<SaveOptions>(e, saveOptions))
            .ToList();
        return Save(pairs);
    }

    /// <summary>
    /// Save a list of entities together in a transaction.
    /// </summary>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    public List<int> Save(List<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        var callLog = Log.Fn<List<int>>(timer: true);
        logStore.Add("save-data", Log);
        return callLog.ReturnAsOk(Entities.SaveEntity(entityOptionPairs));
    }

    public void Save(List<IContentType> contentTypes, SaveOptions saveOptions)
        => ContentType.ExtendSaveContentTypes(contentTypes, saveOptions);

    #endregion

    public int? GetParentAppId(string parentAppGuid, int parentAppId)
        => SqlDb.ToSicEavApps.Count(a => a.Name == parentAppGuid) switch
        {
            0 => throw new ArgumentException(
                $"ParentApp missing. Can't find app:{parentAppGuid}. Import ParentApp first."),
            1 => SqlDb.ToSicEavApps.Single(a => a.Name == parentAppGuid).AppId,
            // we have more apps with requested guid
            _ => SqlDb.ToSicEavApps.Count(a => a.Name == parentAppGuid && a.AppId == parentAppId) switch
            {
                0 => throw new ArgumentException(
                    $"ParentApp is missing. Can't find app with guid:{parentAppGuid} and AppId:{parentAppId}. More apps are with guid:{parentAppGuid} but neither has AppId:{parentAppId}. Can't import."),
                1 => SqlDb.ToSicEavApps.Single(a => a.Name == parentAppGuid && a.AppId == parentAppId).AppId,
                _ => throw new ArgumentException(
                    $"ParentApp is missing. Can't find app with guid:{parentAppGuid} and AppId:{parentAppId}. More apps are with guid:{parentAppGuid} and AppId:{parentAppId}. Can't import.")
            }
        };
}