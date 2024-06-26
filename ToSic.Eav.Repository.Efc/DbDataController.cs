﻿using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Internal.Compression;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Repository.Efc;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class DbDataController : ServiceBase, IStorage, IAppIdentity
{

    #region Properties like AppId, ZoneId, UserName etc.

    /// <summary>
    /// AppId of this whole Context
    /// </summary>
    public int AppId => _appId == Constants.AppIdEmpty ? Constants.MetaDataAppId : _appId;
    public int[] AppIds => _parentAppId == null ? [AppId] : [AppId, _parentAppId.Value];
    private int _appId;

    /// <summary>
    /// ZoneId of this whole Context
    /// </summary>
    public int ZoneId => _zoneId == 0 ? Constants.DefaultZoneId : _zoneId;
    private int _zoneId;

    private const string UserNameUnknown = "unresolved(eav)";
    private string _userName;
    /// <summary>
    /// Current UserName. Used for ChangeLog
    /// </summary>
    public string UserName
    {
        get
        {
            if (_userName != null) return _userName;
            try
            {
                // try to get using dependency injection
                _userName = _userLazy.Value?.IdentityToken ?? UserNameUnknown;
            }
            catch
            {
                _userName = UserNameUnknown;
            }
            return _userName;
        }
    }

    public List<LogItem> ImportLogToBeRefactored { get; } = [];

    public int? ParentAppId { get; set; }
    private int? _parentAppId;

    #endregion

    #region Constructor and Init

    public DbDataController(
        EavDbContext dbContext,
        LazySvc<Efc11Loader> efcLoaderLazy,
        LazySvc<IUser> userLazy,
        AppsCacheSwitch appsCache,
        Generator<JsonSerializer> jsonSerializerGenerator,
        ILogStore logStore,
        LazySvc<Compressor> compressor,
        DataBuilder builder
    ) : base("Db.Data")
    {
        ConnectLogs([
            _efcLoaderLazy = efcLoaderLazy,
            _userLazy = userLazy,
            _appsCache = appsCache,
            _logStore = logStore,
            SqlDb = dbContext,
            JsonSerializerGenerator = jsonSerializerGenerator,
            _compressor = compressor,
            _builder = builder
        ]);
        SqlDb.AlternateSaveHandler += SaveChanges;
    }

    private readonly DataBuilder _builder;
    private readonly LazySvc<Efc11Loader> _efcLoaderLazy;
    private readonly LazySvc<IUser> _userLazy;
    private readonly AppsCacheSwitch _appsCache;
    private readonly ILogStore _logStore;
    private readonly LazySvc<Compressor> _compressor;

    public EavDbContext SqlDb { get; }
    internal Generator<JsonSerializer> JsonSerializerGenerator { get; }

    /// <summary>
    /// Set ZoneId, AppId and ParentAppId on current context.
    /// </summary>
    /// <param name="appState"></param>
    /// <returns></returns>
    public DbDataController Init(IAppStateInternal appState)
    {
        _parentAppId = appState.ParentAppState?.AppId;
        return Init(appState.ZoneId, appState.AppId);
    }

    /// <summary>
    /// Set ZoneId and AppId on current context.
    /// </summary>
    public DbDataController Init(int? zoneId, int? appId)
    {
        // If nothing is supplied, use defaults
        if (!zoneId.HasValue && !appId.HasValue)
        {
            _zoneId = Constants.DefaultZoneId;
            _appId = Constants.MetaDataAppId;
            return this;
        }

        // If only AppId is supplied, look up it's zone and use that
        if (!zoneId.HasValue && appId.HasValue)
        {
            var zoneIdOfApp = SqlDb.ToSicEavApps.Where(a => a.AppId == appId.Value).Select(a => (int?)a.ZoneId).SingleOrDefault();
            if (!zoneIdOfApp.HasValue)
                throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
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
                throw new ArgumentException("App with id " + appId.Value + " doesn't exist.", nameof(appId));
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
    /// Also Creates an initial ChangeLog (used by SQL Server for Auditing).
    /// If items were modified, Cache is purged on current Zone/App
    /// </summary>
    public int SaveChanges(bool acceptAllChangesOnSuccess, EavDbContext.SaveChangesEvent baseEvent)
    {
        if (_appId == Constants.AppIdEmpty)
            throw new("SaveChanges with AppId 0 not allowed.");

        Versioning.GetChangeLogId();

        var modifiedCount = baseEvent(acceptAllChangesOnSuccess);

        if (modifiedCount != 0)
            PurgeAppCacheIfReady();

        return modifiedCount;
    }

    private void PurgeAppCacheIfReady() => Log.Do($"{_purgeAppCacheOnSave}", () =>
    {
        if (_purgeAppCacheOnSave) _appsCache.Value.Purge(this);
    });

    #endregion

    #region Shorthand for do & save

    internal void DoAndSave(Action action, string message = null)
    {
        var l = Log.Fn(message: message, timer: true);
        action.Invoke();
        SqlDb.SaveChanges();
        l.Done();
    }


    internal void DoAndSaveWithoutChangeDetection(Action action, string message = null) => Log.Do(timer: true, message: message, action: l =>
    {
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
        }
    });


    public void DoInTransaction(Action action)
    {
        var randomId = Guid.NewGuid().ToString().Substring(0, 4);
        var ownTransaction = SqlDb.Database.CurrentTransaction == null ? SqlDb.Database.BeginTransaction() : null;
        Log.Do(timer: true, message: $"id:{randomId} - create new trans:{ownTransaction != null}", action: l =>
        {
            try
            {
                action.Invoke();
                ownTransaction?.Commit();
                l.A($"Transaction {randomId} - completed"); // adds ok to end of block
                return "transaction ok"; // adds ok to top of block
            }
            catch (Exception e)
            {
                ownTransaction?.Rollback();
                l.A($"Transaction {randomId} failed / rollback");
                l.A("transaction failed / rollback");
                l.Ex(e);
                throw;
            }
        });
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
    /// Otherwise the loader would be blocked from getting intermediate data while we're running changes. 
    /// </summary>
    public IRepositoryLoader Loader => _loader ??= _efcLoaderLazy.Value.UseExistingDb(SqlDb);
    private IRepositoryLoader _loader;

    public void DoWhileQueuingVersioning(Action action) => Versioning.DoAndSaveHistoryQueue(action);
    public void DoWhileQueueingRelationships(Action action) => Relationships.DoWhileQueueingRelationships(action);

    public List<int> Save(List<IEntity> entities, SaveOptions saveOptions)
    {
        var callLog = Log.Fn<List<int>>(timer: true);
        _logStore.Add("save-data", Log);
        return callLog.ReturnAsOk(Entities.SaveEntity(entities, saveOptions));
    }

    public void Save(List<IContentType> contentTypes, SaveOptions saveOptions)
        => ContentType.ExtendSaveContentTypes(contentTypes, saveOptions);

    #endregion

    public int? GetParentAppId(string parentAppGuid, int parentAppId)
    {
        switch (SqlDb.ToSicEavApps.Count(a => a.Name == parentAppGuid))
        {
            case 0:
                throw new ArgumentException($"ParentApp is missing. Can't find app with guid:{parentAppGuid}. Please import ParentApp first.");
            case 1:
                return SqlDb.ToSicEavApps.Single(a => a.Name == parentAppGuid).AppId;
        }

        // we have more apps with requested guid
        switch (SqlDb.ToSicEavApps.Count(a => a.Name == parentAppGuid && a.AppId == parentAppId))
        {
            case 0:
                throw new ArgumentException($"ParentApp is missing. Can't find app with guid:{parentAppGuid} and AppId:{parentAppId}. More apps are with guid:{parentAppGuid} but nither has AppId:{parentAppId}. Can't import.");
            case 1:
                return SqlDb.ToSicEavApps.Single(a => a.Name == parentAppGuid && a.AppId == parentAppId).AppId;
        }

        throw new ArgumentException($"ParentApp is missing. Can't find app with guid:{parentAppGuid} and AppId:{parentAppId}. More apps are with guid:{parentAppGuid} and AppId:{parentAppId}. Can't import.");
    }
}