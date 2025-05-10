using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Serialization.Internal;

/// <summary>
/// Constructor for inheriting classes
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class SerializerBase(SerializerBase.MyServices services, string logName) : ServiceBase<SerializerBase.MyServices>(services, logName), IDataSerializer
{
    #region MyServices

    public class MyServices(ITargetTypes metadataTargets, DataBuilder dataBuilder, IAppReaderFactory appStates, object[] connect = default)
        : MyServicesBase(connect: [metadataTargets, dataBuilder, appStates, ..connect ?? []])
    {
        public DataBuilder DataBuilder { get; } = dataBuilder;

        public ITargetTypes MetadataTargets { get; } = metadataTargets;
        public IAppReaderFactory AppStates { get; } = appStates;
    }

    #endregion

    #region Constructor / DI

    private readonly IAppReadContentTypes _globalAppOrNull = services.AppStates.GetSystemPreset(nullIfNotLoaded: true);

    public ITargetTypes MetadataTargets { get; } = services.MetadataTargets;


    public void Initialize(IAppReader appReader)
    {
        AppReaderOrNull = appReader;
        AppId = appReader.AppId;
    }

    protected int AppId;
    private IEnumerable<IContentType> _types;
    public void Initialize(int appId, IEnumerable<IContentType> types, IEntitiesSource allEntities)
    {
        AppId = appId;
        _types = types;
        _relList = allEntities;
    }

    #endregion

    #region Special Loggers

    public void ConfigureLogging(LogSettings settings)
    {
        var l = Log.Fn(settings.ToString());
        _disableLogDetails = Log.IfDetails(settings) == null;
        _disableLogSummary = Log.IfSummary(settings) == null;
        l.Done();
    }

    public new ILog Log => base.Log;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog LogDsDetails => _disableLogDetails ? null : Log;
    private bool _disableLogDetails = false;

    internal ILog LogDsSummary => _disableLogSummary ? null : Log;
    private bool _disableLogSummary = false;

    #endregion

    public IAppReader AppReaderOrError => AppReaderOrNull ?? throw new("cannot use app in serializer without initializing it first, make sure you call Initialize(...)");
    protected IAppReader AppReaderOrNull { get; private set; }

    public bool PreferLocalAppTypes = false;

    protected IContentType GetContentType(string staticName)
    {
        var l = LogDsDetails.Fn<IContentType>($"name: {staticName}, preferLocal: {PreferLocalAppTypes}", timer: true);
        // There is a complex lookup we must protocol to better detect issues, which is why we assemble a message
        var msg = "";

        // If local type is preferred, use the App accessor,
        // this will also check the global types internally
        // ReSharper disable once InvertIf
        if (PreferLocalAppTypes)
        {
            var type = AppReaderOrError.GetContentType(staticName);
            if (type != null)
                return l.Return(type, $"app: found");
            msg += "app: not found, ";
        }

        var globalType = _globalAppOrNull?.GetContentType(staticName);

        if (globalType != null)
            return l.Return(globalType, $"{msg}global: found");
        msg += "global: not found, ";

        if (_types != null)
        {
            msg += "local-list: ";
            globalType = _types.FirstOrDefault(t => t.NameId == staticName);
        }
        else
        {
            msg += "app: ";
            globalType = AppReaderOrError.GetContentType(staticName);
        }

        return l.Return(globalType, $"{msg}{(globalType == null ? "not " : "")}found");
    }

    protected IContentType GetTransientContentType(string name, string nameId)
    {
        var transientContentType = Services.DataBuilder.ContentType.Transient(AppId, name, nameId);
        return DeserializationSettings?.ContentTypeProvider?.LazyTypeGenerator(AppId, name, nameId, transientContentType)
               ?? transientContentType;
    }

    /// <summary>
    /// Ability to inject a different TransientContentTypeGenerator and other parameters
    /// just for deserialization
    /// </summary>
    internal JsonDeSerializationSettings DeserializationSettings { get; set; } = null;

    protected IEntity Lookup(int entityId) => AppReaderOrError.List.FindRepoId(entityId); // should use repo, as we're often serializing unpublished entities, and then the ID is the Repo-ID

    public abstract string Serialize(IEntity entity);

    public string Serialize(int entityId) => Serialize(Lookup(entityId));

    public Dictionary<int, string> Serialize(List<int> entities) => entities.ToDictionary(x => x, Serialize);

    public Dictionary<int, string> Serialize(List<IEntity> entities) => entities.ToDictionary(e => e.EntityId, Serialize);

    protected IEntitiesSource LazyRelationshipLookupList => _relList ??= AppReaderOrError.GetCache();
    private IEntitiesSource _relList;

}