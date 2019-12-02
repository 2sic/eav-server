using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// This contains the structure / definition of a query, which was originally stored in an <see cref="IEntity"/>
    /// </summary>
    [PublicApi]
    public class QueryDefinition: EntityBasedType
    {
        /// <summary>
        /// The appid inside which the query will run, _not where it is stored!_ <br/>
        /// This can differ, because certain global queries (stored in the global app) will run in a specific app - for example to retrieve all ContentTypes of that app.
        /// </summary>
        public int AppId;

        /// <summary>
        /// The parts of the query
        /// </summary>
        public List<QueryPartDefinition> Parts
            => _parts ?? (_parts = Entity.Metadata
                   .Where(m => m.Type.Name == Constants.QueryPartTypeName)
                   .Select(e => new QueryPartDefinition(e))
                   .ToList());
        private List<QueryPartDefinition> _parts;

        public IList<Connection> Connections => _connections ?? (_connections = Queries.Connections.Deserialize(WiringRaw));
        private IList<Connection> _connections;

        public string WiringRaw => Get(Constants.QueryStreamWiringAttributeName, "");

        [PrivateApi]
        public QueryDefinition(IEntity header, int appId): base(header)
        {
            if (appId == 0)
                appId = header.AppId;
            AppId = appId;
        }
    }
}
