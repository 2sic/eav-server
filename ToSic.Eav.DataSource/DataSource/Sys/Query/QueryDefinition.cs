using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// This contains the structure / definition of a query, which was originally stored in an <see cref="IEntity"/>
/// </summary>
[PrivateApi("Till v17 was InternalApi_DoNotUse_MayChangeWithoutNotice - this is just fyi")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial record QueryDefinition : RecordOfEntityWithLog
{
    public QueryDefinition(IEntity headerEntity, int appId, List<QueryPartDefinition> parts, ILog parentLog)
        : base(headerEntity, parentLog, "DS.QDef")
    {
        AppId = appId;
        Parts = parts;
    }

    /// <summary>Content-Type name of the queryPart Content-Type</summary>
    [PrivateApi] internal static readonly string TypeName = "DataPipeline";
    /// <summary>
    /// The appid inside which the query will run, _not where it is stored!_ <br/>
    /// This can differ, because certain global queries (stored in the global app) will run in a specific app - for example to retrieve all ContentTypes of that app.
    /// </summary>
    public int AppId => field == KnownAppsConstants.AppIdEmpty
        ? Entity.AppId
        : field;

    public string Name => GetThis("error no name");

    /// <summary>
    /// The parts of the query
    /// </summary>
    public List<QueryPartDefinition> Parts { get; }


    /// <summary>
    /// Connections used in the query to map various DataSource Out-Streams to various other DataTarget In-Streams
    /// </summary>
    [field: AllowNull, MaybeNull]
    public IList<Connection> Connections => field ??= Sys.Query.Connections.Deserialize(StreamWiring);

    /// <summary>
    /// The connections as they are serialized in the Entity
    /// </summary>
    [PrivateApi]
    public string StreamWiring => GetThis("");

}