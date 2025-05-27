using System.Collections.Immutable;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Metadata;
using ToSic.Eav.StartUp;
using ToSic.Lib.Caching;
using ToSic.Lib.Services;
using static ToSic.Eav.Constants;


namespace ToSic.Eav.Apps.State;

partial class AppState
{
    /// <summary>
    /// The builder must be a subclass of AppState, so it can access its private properties.
    /// </summary>
    internal class AppStateBuilder(IAppReaderFactory appReaderFactory) : ServiceBase("App.SttBld"), IAppStateBuilder
    {
        #region Constructor / DI / Init (2 variants)

        public IAppStateBuilder Init(IAppStateCache appState)
        {
            AppState = appState;
            return this;
        }

        public IAppStateBuilder InitForPreset()
        {
            AppState = new AppState(new(null, false, false), PresetIdentity, PresetName, Log);
            MemoryCacheService.Notify(AppState);
            return this;
        }

        public IAppStateBuilder InitForNewApp(ParentAppState? parentApp, IAppIdentity identity, string nameId, ILog parentLog)
        {
            AppState = new AppState(parentApp, identity, nameId, parentLog);
            MemoryCacheService.Notify(AppState);
            return this;
        }

        public IAppStateCache AppState
        {
            get => field ?? throw new("Can't use before calling some init");
            private set;
        }

        public IAppReader Reader => field ??= appReaderFactory.ToReader(AppState);

        #endregion

        #region Loading

        private static bool _loggedLoadToBootLog = false;


        public void Load(string message, Action<IAppStateCache> loader)
        {
            var st = (AppState)AppState;
            var msg = $"zone/app:{st.Show()} - Hash: {st.GetHashCode()}";
            var l = Log.Fn($"{msg} {message}", timer: true);
            var bl = _loggedLoadToBootLog ? null : BootLog.Log.Fn($"{msg} {message}", timer: true);

            var lState = st.Log.Fn(message, timer: true);
            try
            {
                // first set a lock, to ensure that only one update/load is running at the same time
                lock (this)
                {
                    var lInLock = l.Fn($"loading: {st.Loading} (app loading start in lock)");
                        // only if loading is true will the AppState object accept changes
                        st.Loading = true;
                        loader.Invoke(st);
                        st.CacheResetTimestamp("load complete");
                        _ = EnsureNameAndFolderInitialized();
                        if (!st.FirstLoadCompleted) st.FirstLoadCompleted = true;
                    lInLock.Done($"done - dynamic load count: {st.DynamicUpdatesCount}");
                }
            }
            catch (Exception ex)
            {
                lState.Ex(ex);
                st.IsHealthy = false;
                st.HealthMessage += $"Error while building App state in memory, probably during initial load. {ex.Message}\n";
            }
            finally
            {
                // set loading to false again, to ensure that AppState won't accept changes
                st.Loading = false;
                lState.Done();
            }

            bl.Done();
            // only keep logging for the preset and first app, then stop.
            if (st.AppId != PresetAppId)
                _loggedLoadToBootLog = true;
            l.Done();
        }

        #endregion

        #region Name, Folder, Initialize

        public void SetNameAndFolder(string name, string folder)
        {
            var st = (AppState)AppState;
            st.Name = name;
            st.Folder = folder;
        }

        private bool EnsureNameAndFolderInitialized()
        {
            var st = (AppState)AppState;
            var l = st.Log.Fn<bool>();
            // Before we do anything, check primary App
            // Otherwise other checks (like is name empty) will fail, because it's not empty
            // This is necessary, because it does get loaded with real settings
            // But we must override them to always be the same.
            if (st.NameId == PrimaryAppGuid)
            {
                st.Folder = PrimaryAppFolder;
                st.Name = PrimaryAppName;
                return l.ReturnTrue($"Primary App. Name: {st.Name}, Folder:{st.Folder}");
            }

            // Only do something if Name or Folder are still invalid
            if (!string.IsNullOrWhiteSpace(st.Name) && !string.IsNullOrWhiteSpace(st.Folder))
                return l.ReturnFalse($"No change. Name: {st.Name}, Folder:{st.Folder}");

            // If the loader wasn't able to fill name/folder, then the data was not a json
            // so we must try to fix this now
            l.A("Trying to load Name/Folder from App package entity");
            // note: we sometimes have a (still unsolved) problem, that the AppConfig is generated multiple times
            // so the OfType().OrderBy() should ensure that we really only take the first=oldest one.
            var config = st.List
                .OfType(AppLoadConstants.TypeAppConfig)
                .OrderBy(e => e.EntityId)
                .FirstOrDefault();
            if (st.Name.IsEmptyOrWs()) st.Name = config?.Get<string>(AppLoadConstants.FieldName);
            if (st.Folder.IsEmptyOrWs()) st.Folder = config?.Get<string>(AppLoadConstants.FieldFolder);

            // Last corrections for the DefaultApp "Content"
            if (st.NameId == DefaultAppGuid)
            {
                // Always set the Name if we are on the content or primary app
                st.Name = ContentAppName;
                // Only set the folder if not over-configured since it can change in v13+
                if (st.Folder.IsEmptyOrWs()) st.Folder = ContentAppFolder;
            }

            return l.ReturnTrue($"Name: {st.Name}, Folder:{st.Folder}");
        }


        #endregion

        #region Entity Index

        /// <summary>
        /// Check if a new entity previously had a draft, and remove that
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="log">To optionally disable logging, in case it would overfill what we're seeing!</param>
        private bool RemoveObsoleteDraft(IEntity newEntity, bool log)
        {
            var st = (AppState)AppState;
            var l = log ? st.Log.Fn<bool>() : null;
            var previous = st.Index.TryGetValue(newEntity.EntityId, out var prev) ? prev : null;
            var draftEnt = st.GetDraft(previous);

            // check if we went from draft-branch to published, because in this case, we have to remove the last draft
            const string noChangePrefix = "remove obsolete draft - no change:";

            // if we have previous, then just show a message and exit
            if (previous == null)
                return l.ReturnFalse($"{noChangePrefix} previous is null => new will be added to cache");

            // if previous wasn't published, so we couldn't have had a branch
            if (!previous.IsPublished)
                return l.ReturnFalse($"{noChangePrefix} previous not published => new will replace in cache");

            // if new entity isn't published, so we're not switching "back"
            if (!newEntity.IsPublished && draftEnt == null)
                return l.ReturnFalse($"{noChangePrefix} new copy not published, and no draft exists => new will replace in cache");

            var draftId = draftEnt?.RepositoryId;
            if (draftId == null)
                return l.ReturnFalse("remove obsolete draft - no draft, won't remove");
            
            st.Index.Remove(draftId.Value);
            return l.ReturnTrue($"remove obsolete draft - found draft, will remove {draftId.Value}");
        }

        /// <summary>
        /// Reconnect / wire drafts to the published item
        /// </summary>
        private bool MapDraftToPublished(Entity newEntity, int? publishedId, bool log)
        {
            var st = AppState;
            var l = log ? st.Log.Fn<bool>() : null;
            // fix: #3070, publishedId sometimes has value 0, but that one should not be used
            if (newEntity.IsPublished || !publishedId.HasValue || publishedId.Value == 0) return l.ReturnFalse();

            l.A($"map draft to published for new: {newEntity.EntityId} on {publishedId}");

            // Published Entity is already in the Entities-List as EntityIds is validated/extended before and Draft-EntityID is always higher as Published EntityId
            //newEntity.PublishedEntity = Index[publishedId.Value];
            //((Entity)newEntity.PublishedEntity).DraftEntity = newEntity;
            newEntity.EntityId = publishedId.Value; // this is not immutable, but probably not an issue because it is not in the index yet
            return l.ReturnTrue();
        }


        #endregion

        #region Entities

        /// <summary>
        /// Reset all item storages and indexes
        /// </summary>
        public void RemoveAllItems()
        {
            var st = (AppState)AppState;
            var l = Log.Fn($"for a#{st.AppId}");
            if (!st.Loading)
                throw new("trying to init metadata, but not in loading state. set that first!");
            st.Log.A("remove all items");
            st.Index.Clear();
            st._metadataManager.Reset();
            l.Done();
        }


        /// <summary>
        /// Removes an entity from the cache. Should only be used by EAV code
        /// </summary>
        /// <remarks>
        /// Introduced in v15.05 to reduce work on entity delete.
        /// In past we PurgeApp in whole on each entity delete.
        /// This should be much faster, but side effects are possible.
        /// </remarks>
        [PrivateApi("Only internal use")]
        public void RemoveEntities(int[] repositoryIds, bool log)
        {
            //AppState.Remove(repositoryIds, log);
            if (repositoryIds == null || repositoryIds.Length == 0) return;
            Load("", appState =>
            {
                var st = (AppState)appState;
                foreach (var id in repositoryIds)
                {
                    // Remove any drafts that are related if necessary
                    if (st.Index.TryGetValue(id, out var oldEntity))
                        st._metadataManager.Register(oldEntity, false);

                    st.Index.Remove(id);

                    if (log) Log.A($"removed entity {id}");
                }
            });
        }

        /// <summary>
        /// Add an entity to the cache. Should only be used by EAV code
        /// </summary>
        public void Add(IEntity newEntity, int? publishedId, bool log)
        {
            var st = (AppState)AppState;
            if (!st.Loading)
                throw new("trying to add entity, but not in loading state. set that first!");

            if (newEntity.RepositoryId == 0)
                throw new("Entities without real ID not supported yet");

            RemoveObsoleteDraft(newEntity, log);
            _ = MapDraftToPublished(newEntity as Entity, publishedId, log); // this is not immutable, but probably not an issue because it is not in the index yet
            st.Index[newEntity.RepositoryId] = newEntity; // add like this, it could also be an update
            st._metadataManager.Register(newEntity, true);

            if (st.FirstLoadCompleted)
                st.DynamicUpdatesCount++;

            if (log)
                st.Log.A($"added entity {newEntity.EntityId} for published {publishedId}; dyn-update#{st.DynamicUpdatesCount}");
        }

        #endregion

        #region Metadata

        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        public void InitMetadata()
        {
            var st = (AppState)AppState;
            if (!st.Loading)
                throw new("Trying to init metadata, but App is not in loading state.");
            if (st._appContentTypesFromRepository != null)
                throw new("Can't init metadata if content-types are already set");
            st._metadataManager = new(st, st);

            st.Metadata = st.GetMetadataOf(TargetTypes.App, st.AppId, "App (" + st.AppId + ") " + st.Name + " (" + st.Folder + ")");
        }

        #endregion

        #region Content Types

        /// <summary>
        /// The second init-command
        /// Load content-types
        /// </summary>
        public void InitContentTypes(IList<IContentType> contentTypes)
        {
            var st = (AppState)AppState;
            var l = st.Log.Fn($"contentTypes count: {contentTypes?.Count}", timer: true);

            if (!st.Loading)
                throw new("trying to set content-types, but not in loading state. set that first!");

            if (st._metadataManager == null || st.Index.Any())
                throw new(
                    "can't set content types before setting Metadata manager, or after entities-list already exists");

            if (contentTypes == null)
                throw new ArgumentException(@"contentTypes must always be non-null", nameof(contentTypes));

            st._appTypeMap = contentTypes
                // temp V11.01 - all the local content-types in the /system/ folder have id=0
                // will filter out for now, because otherwise we get duplicate keys-errors
                // believe this shouldn't be an issue, as it only seems to be used in fairly edge-case export/import
                // situations which the static types shouldn't be used for, as they are json-typed
                .Where(x => x.Id != 0 && x.Id < FsDataConstants.GlobalContentTypeMin)
                .ToImmutableDictionary(x => x.Id, x => x.NameId);
            
            st._appContentTypesFromRepository = RemoveAliasesForGlobalTypes(st, contentTypes);
            
            // build types by name
            st._appTypesByName = BuildCacheForTypesByName(st._appContentTypesFromRepository, st.Log);

            l.Done();
        }

        private static IDictionary<string, IContentType> BuildCacheForTypesByName(IImmutableList<IContentType> allTypes, ILog log)
        {
            var l = log.Fn<IDictionary<string, IContentType>>(message: $"build cache for type names for {allTypes.Count} items", timer: true);

            var appTypesByName = new Dictionary<string, IContentType>(StringComparer.InvariantCultureIgnoreCase);

            // add with static name - as the primary key
            foreach (var type in allTypes)
                if (!appTypesByName.ContainsKey(type.NameId))
                    appTypesByName.Add(type.NameId, type);

            // add with nice name, if not already added
            foreach (var type in allTypes)
                if (!appTypesByName.ContainsKey(type.Name))
                    appTypesByName.Add(type.Name, type);

            return l.Return(appTypesByName);
        }

        private static IImmutableList<IContentType> RemoveAliasesForGlobalTypes(AppState st, IList<IContentType> appTypes)
        {
            var globTypeNames = st.ParentApp.ContentTypes.Select(t => t.NameId);
            return appTypes.Where(t =>
                        !t.AlwaysShareConfiguration // keep all locally defined types
                        || !globTypeNames.Contains(t.NameId)    // for non-local: keep all which globally are not overwritten
                )
                .ToImmutableList();
        }


        #endregion
    }
}

