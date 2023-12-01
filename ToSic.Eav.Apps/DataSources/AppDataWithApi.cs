using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.Caching;
using ToSic.Eav.Generics;
using ToSic.Eav.Metadata;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps;

/// <summary>
/// The Data object on an App. It's also a data-source of type <see cref="Eav.DataSources.App"/>,
/// so it has many streams, one for each content-type so you can use it in combination with other DataSources. <br/>
/// The special feature is that it also has methods for data-manipulation,
/// including Create, Update and Delete
/// </summary>
[PublicApi_Stable_ForUseInYourCode]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class AppData: Eav.DataSources.App, IAppData
{
    private readonly LazySvc<IDataSourceCacheService> _dsCacheSvc;

    #region Constructor stuff

    public AppData(MyServices services, LazySvc<SimpleDataController> dataController, LazySvc<IDataSourceCacheService> dsCacheSvc) : base(services)
    {
        ConnectServices(
            DataController = dataController.SetInit(dc => dc.Init(ZoneId, AppId, false)),
            _dsCacheSvc = dsCacheSvc
        );
    }

    #endregion

    /// <summary>
    /// Get a correctly instantiated instance of the simple data controller once needed.
    /// </summary>
    /// <returns>An data controller to create, update and delete entities</returns>
    private LazySvc<SimpleDataController> DataController { get; }

    /// <inheritdoc />
    public IEntity Create(string contentTypeName, Dictionary<string, object> values, string userName = default, ITarget target = default)
    {
        var l = Log.Fn<IEntity>(contentTypeName);
        // Ensure case insensitive
        values = values.ToInvariant();

        if (!string.IsNullOrEmpty(userName)) ProvideOwnerInValues(values, userName); // userName should be in 2sxc user IdentityToken format (eg 'dnn:user=N')
        var ids = DataController.Value.Create(contentTypeName, new List<Dictionary<string, object>> { values }, target);
        var id = ids.FirstOrDefault();
        FlushDataSnapshot();
        // try to find it again (AppState.List contains also draft items)
        var created = AppState.List.One(id);
        return l.Return(created, $"{created?.EntityId}/{created?.EntityGuid}");
    }

    private static void ProvideOwnerInValues(Dictionary<string, object> values, string userIdentityToken)
    {
        // userIdentityToken is not simple 'userName' string, but 2sxc user IdentityToken structure (eg 'dnn:user=N')
        if (values.ContainsKey(Attributes.EntityFieldOwner)) return;
        values.Add(Attributes.EntityFieldOwner, userIdentityToken);
    }

    /// <inheritdoc />
    public IEnumerable<IEntity> Create(string contentTypeName, IEnumerable<Dictionary<string, object>> multiValues, string userName = default)
    {
        var l = Log.Fn<IEnumerable<IEntity>>($"app create many ({multiValues.Count()}) new entities of type:{contentTypeName}");
        // ensure case insensitive
        multiValues = multiValues.Select(mv => mv.ToInvariant()).ToList();

        if (!string.IsNullOrEmpty(userName))
            foreach (var values in multiValues)
                ProvideOwnerInValues(values, userName); // userName should be in 2sxc user IdentityToken format (eg 'dnn:user=N')

        var ids = DataController.Value.Create(contentTypeName, multiValues);
        FlushDataSnapshot();
        var created = List.Where(e => ids.Contains(e.EntityId)).ToList();
        return l.Return(created, $"{created.Count}");
    }

    /// <inheritdoc />
    public void Update(int entityId, Dictionary<string, object> values, string userName = default)
    {
        var l = Log.Fn($"app update i:{entityId}");
        // FYI: userName is not used (to change owner of updated entity).
        DataController.Value.Update(entityId, values);
        FlushDataSnapshot();
        l.Done();
    }


    /// <inheritdoc />
    public void Delete(int entityId, string userName = default)
    {
        var l = Log.Fn($"app delete i:{entityId}");
        // FYI: userName is not used (to change owner of deleted entity).
        DataController.Value.Delete(entityId);
        FlushDataSnapshot();
        l.Done();
    }

    /// <summary>
    /// All 2sxc data is always snapshot, so read will only run a query once and keep it till the objects are killed.
    /// If we do updates or perform other changes, we must clear the current snapshot so subsequent access will result
    /// in the new data. 
    /// </summary>
    private void FlushDataSnapshot()
    {
        // Purge the list and parent lists - must happen first, as otherwise the list-access will be interrupted
        _dsCacheSvc.Value.Flush(this, true);
        Reset();
    }

    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TKey>(int targetType, TKey key, string contentTypeName = null) 
        => AppState.GetMetadata(targetType, key, contentTypeName);

    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null) 
        => AppState.GetMetadata(targetType, key, contentTypeName);

    /// <inheritdoc />
    public IEnumerable<IEntity> GetCustomMetadata<TKey>(TKey key, string contentTypeName = null)
        => AppState.GetMetadata(TargetTypes.Custom, key, contentTypeName);
}