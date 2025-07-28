﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Loaders;
using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Persistence.Efc.Sys.Services;
using ToSic.Eav.Persistence.Sys.Logging;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbContentTypes;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;
using ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
using ToSic.Eav.Repository.Efc.Sys.DbParts;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Users;
using ToSic.Sys.Utils.Compression;

namespace ToSic.Eav.Repository.Efc.Sys.DbStorage;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DbStorage(
    EavDbContext dbContext,
    LazySvc<EfcAppLoaderService> efcLoaderLazy,
    LazySvc<IUser> userLazy,
    IAppCachePurger appsCache,
    Generator<JsonSerializer> jsonSerializerGenerator,
    ILogStore logStore,
    LazySvc<Compressor> compressor,
    DataBuilder builder,
    DataImportLogSettings importLogSettings,
    ISysFeaturesService features)
    : ServiceBase("Db.Data",
        connect: [efcLoaderLazy, userLazy, appsCache, logStore, dbContext, jsonSerializerGenerator, compressor, builder, importLogSettings, features]
    ), IStorage, IAppIdentity
{

    #region Properties like AppId, ZoneId, UserName etc.

    /// <summary>
    /// AppId of this whole Context
    /// </summary>
    public int AppId => _appId == KnownAppsConstants.AppIdEmpty ? KnownAppsConstants.MetaDataAppId : _appId;
    private int _appId;

    /// <summary>
    /// AppIds with parent AppId if it's set.
    /// Will only have another ID if a non-default parent is used, e.g. in inherited apps.
    /// </summary>
    public int[] AppIds => ParentAppId == null ? [AppId] : [AppId, ParentAppId.Value];

    /// <summary>
    /// ZoneId of this whole Context
    /// </summary>
    public int ZoneId => _zoneId == 0 ? KnownAppsConstants.DefaultZoneId : _zoneId;
    private int _zoneId;

    private const string UserNameUnknown = "unresolved(eav)";

    /// <summary>
    /// Current UserName. Used for TransactionId
    /// </summary>
    [field: AllowNull, MaybeNull]
    public string UserIdentityToken
    {
        get
        {
            if (field != null)
                return field;
            try
            {
                // try to get using dependency injection
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract - Paranoid...
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

    [field: AllowNull, MaybeNull]
    internal DbVersioning Versioning => field ??= new(this, compressor);
    [field: AllowNull, MaybeNull]
    internal DbEntity Entities => field ??= new(this, builder);
    [field: AllowNull, MaybeNull]
    internal DbValue Values => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbAttribute Attributes => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbRelationship Relationships => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbAttributeSet AttribSet => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbPublishing Publishing => field ??= new(this, builder);
    [field: AllowNull, MaybeNull]
    internal DbDimensions Dimensions => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbZone Zone => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbApp App => field ??= new(this);
    [field: AllowNull, MaybeNull]
    internal DbContentType ContentType => field ??= new(this);

    #endregion


    public List<Message> ImportLogToBeRefactored { get; } = [];


    #region Constructor and Init

    private static bool _loggedToBootLog;

    [field: AllowNull, MaybeNull]
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

    internal ISysFeaturesService Features => features;

    #region Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings { get => field ??= importLogSettings.GetLogSettings(); set; } = new();

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDetails
    {
        get => field ??= Log.IfDetails(LogSettings);
        set;
    }

    internal ILog? LogSummary
    {
        get => field ??= Log.IfSummary(LogSettings);
        set;
    }

    #endregion


    /// <summary>
    /// New factory setup using the better paradigm.
    /// TODO: try to shift all others to use this mechanism as well.
    /// </summary>
    /// <param name="options"></param>
    public void Setup(StorageOptions options)
    {
        Init(options.ZoneId, options.AppId, options.ParentAppId);

        ConfigureLogging(options.LogSettings);
    }

    // WIP, not final architecture...
    public void ConfigureLogging(LogSettings logSettings)
    {
        var l = Log.Fn($"Settings: {logSettings}");
        // Store settings and reset the loggers, so they retrieve it again next time.
        LogSettings = logSettings;
        LogDetails = null;
        LogSummary = null;

        JsonSerializerGenerator.SetInit(ser => ser.ConfigureLogging(logSettings), allowReplace: true);
        l.Done();
    }


    /// <summary>
    /// Set ZoneId and AppId on current context.
    /// </summary>
    private DbStorage Init(int? zoneId, int? appId, int? parentAppId = default)
    {
        // No matter what scenario we have, always set the parent app id - if given
        // todo: maybe later also try to detect it, if we see the need for it
        ParentAppId = parentAppId;

        // If we don't have a zoneId, ensure we set everything from defaults or lookup
        if (!zoneId.HasValue)
            // If nothing is supplied, use defaults
            if (!appId.HasValue)
            {
                _zoneId = KnownAppsConstants.DefaultZoneId;
                _appId = KnownAppsConstants.MetaDataAppId;
                return this;
            }
            // If only AppId is supplied, look up it's zone and use that
            else
            {
                var zoneIdOfApp = SqlDb.TsDynDataApps
                    .Where(a => a.AppId == appId.Value)
                    .Select(a => (int?)a.ZoneId)
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
            var foundApp = SqlDb.TsDynDataApps.FirstOrDefault(a => a.ZoneId == _zoneId && a.AppId == appId.Value);
            if (foundApp == null)
                throw new ArgumentException($@"App with id {appId.Value} doesn't exist.", nameof(appId));
            _appId = appId.Value;
        }
        else
            _appId = SqlDb.TsDynDataApps.First(a => a.Name == KnownAppsConstants.DefaultAppGuid).AppId;

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
        if (_appId == KnownAppsConstants.AppIdEmpty)
            throw new("SaveChanges with AppId 0 not allowed.");

        Versioning.GetTransactionId();

        var modifiedCount = baseEvent(acceptAllChangesOnSuccess);

        if (modifiedCount != 0)
            PurgeAppCacheIfReady();

        return modifiedCount;
    }

    private void PurgeAppCacheIfReady()
    {
        var l = LogDetails.Fn($"{_purgeAppCacheOnSave}");
        if (_purgeAppCacheOnSave)
            appsCache.Purge(this);
        l.Done();
    }

    #endregion

    #region Shorthand for do & save

    internal void DoAndSave(Action action, string? message = null)
    {
        var l = LogSummary.Fn(message: message, timer: true);
        action.Invoke();
        SqlDb.SaveChanges();
        l.Done();
    }


    internal void DoAndSaveWithoutChangeDetection(Action action, string? message = null)
    {
        var l = LogSummary.Fn(timer: true, message: message);
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
        var log = ownTransaction == null ? LogDetails : LogSummary;
        var l = log.Fn(timer: true, message: $"id:{randomId} - create new trans:{ownTransaction != null}");
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
        var l = LogSummary.Fn(timer: true);
        var before = _purgeAppCacheOnSave;
        _purgeAppCacheOnSave = false;
        action.Invoke();
        _purgeAppCacheOnSave = before;
        l.Done();
    }

    public void DoWithDelayedCacheInvalidation(Action action)
    {
        var l = LogSummary.Fn(timer: true);
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
    [field: AllowNull, MaybeNull]
    public IAppsAndZonesLoaderWithRaw Loader => field ??= efcLoaderLazy.Value.UseExistingDb(SqlDb);

    public void DoWhileQueuingVersioning(Action action)
        => Versioning.DoAndSaveHistoryQueue(action);

    /// <summary>
    /// Save a list of entities together in a transaction.
    /// </summary>
    /// <param name="entityOptionPairs"></param>
    /// <returns></returns>
    public List<EntityIdentity> Save(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        var l = LogDetails.Fn<List<EntityIdentity>>(timer: true);
        logStore.Add("save-data", Log);
        return l.ReturnAsOk(Entities.SaveEntities(entityOptionPairs));
    }

    public void Save(List<IContentType> contentTypes, SaveOptions saveOptions)
        => ContentType.ExtendSaveContentTypes(contentTypes, saveOptions);

    #endregion

    public int GetParentAppId(string parentAppGuid, int parentAppId)
    {
        var appCountWithParentGuid = SqlDb.TsDynDataApps.Count(a => a.Name == parentAppGuid);
        return appCountWithParentGuid switch
        {
            0 => throw new ArgumentException(
                $"ParentApp missing. Can't find app:{parentAppGuid}. Import ParentApp first."),
            1 => SqlDb.TsDynDataApps.Single(a => a.Name == parentAppGuid).AppId,
            // we have more apps with requested guid
            _ => SqlDb.TsDynDataApps.Count(a => a.Name == parentAppGuid && a.AppId == parentAppId) switch
            {
                0 => throw new ArgumentException($"{ErrorMessage()} but neither has AppId:{parentAppId}. Can't import."),
                1 => SqlDb.TsDynDataApps.Single(a => a.Name == parentAppGuid && a.AppId == parentAppId).AppId,
                _ => throw new ArgumentException($"{ErrorMessage()} and AppId:{parentAppId}. Can't import.")
            }
        };

        string ErrorMessage() =>
            $"ParentApp is missing. Can't find app with guid:{parentAppGuid} and AppId:{parentAppId}. '{appCountWithParentGuid}' apps are with guid:{parentAppGuid}";
    }

    public int CreateApp(string guidName, int? inheritAppId = null)
    {
        var l = LogSummary.Fn<int>($"guid:{guidName}, inheritAppId:{inheritAppId}");
        var app = App.AddApp(null, guidName, inheritAppId);
        SqlDb.SaveChanges(); // save is required to ensure AppId is created - required for follow-up changes like EnsureSharedAttributeSets();
        return l.Return(app.AppId, $"Created App with Id:{app.AppId} and Name:{app.Name} in ZoneId:{ZoneId}");
    }

}