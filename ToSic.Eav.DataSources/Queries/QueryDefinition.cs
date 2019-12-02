using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Queries
{
    // Todo: maybe change to entity-based-type?
    public class QueryDefinition: EntityBasedType
    {
        /// <summary>
        /// The appid inside which the query will run (not where it is stored!)
        /// </summary>
        public int AppId; 

        //public IEntity Entity;
        public List<IEntity> Parts => Entity.Metadata.Where(m => m.Type.Name == Constants.QueryPartTypeName).ToList();

        public QueryDefinition(IEntity header, int appId): base(header)
        {
            //Entity = header;
            if (appId == 0)
                appId = header.AppId;
            AppId = appId;// ?? header.AppId;
        }
    }
}
