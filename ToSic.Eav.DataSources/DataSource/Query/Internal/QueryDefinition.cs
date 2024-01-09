using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.Query;

/// <summary>
/// This contains the structure / definition of a query, which was originally stored in an <see cref="IEntity"/>
/// </summary>
[PrivateApi("Till v17 was InternalApi_DoNotUse_MayChangeWithoutNotice - this is just fyi")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class QueryDefinition: EntityBasedWithLog
{
    #region Constants / Field Names

    [PrivateApi]
    internal const string FieldTestParams = "TestParameters";

    [PrivateApi]
    internal const string FieldParams = DataSourceConstants.ParamsSourceName;

    #endregion

    #region Constructor
    [PrivateApi]
    public QueryDefinition(IEntity header, int appId, List<QueryPartDefinition> parts, ILog parentLog) 
        : base(header, parentLog, "DS.QDef")
    {
        if (appId == Constants.AppIdEmpty)
            appId = header.AppId;
        AppId = appId;
        Parts = parts;
    }
    #endregion


    /// <summary>
    /// The appid inside which the query will run, _not where it is stored!_ <br/>
    /// This can differ, because certain global queries (stored in the global app) will run in a specific app - for example to retrieve all ContentTypes of that app.
    /// </summary>
    public int AppId;


    /// <summary>
    /// The parts of the query
    /// </summary>
    public List<QueryPartDefinition> Parts { get; }


    /// <summary>
    /// Connections used in the query to map various DataSource Out-Streams to various other DataTarget In-Streams
    /// </summary>
    public IList<Connection> Connections => _connections ??= Eav.DataSource.Query.Connections.Deserialize(ConnectionsRaw);
    private IList<Connection> _connections;

    /// <summary>
    /// The connections as they are serialized in the Entity
    /// </summary>
    [PrivateApi]
    private string ConnectionsRaw => Get(QueryConstants.QueryStreamWiringAttributeName, "");

}