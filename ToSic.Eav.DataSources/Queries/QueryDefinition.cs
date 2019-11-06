using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Queries
{
    public class QueryDefinition
    {
        /// <summary>
        /// The appid inside which the query will run (not where it is stored!)
        /// </summary>
        public int AppId; 

        public IEntity Header;
        public List<IEntity> Parts => Header.Metadata.Where(m => m.Type.Name == Constants.QueryPartTypeName).ToList();

        public QueryDefinition(IEntity header, int? appId = null)
        {
            Header = header;
            AppId = appId ?? header.AppId;
        }
    }
}
