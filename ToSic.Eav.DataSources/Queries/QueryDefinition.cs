using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Queries
{
    public class QueryDefinition
    {
        public IEntity Header;
        public List<IEntity> Parts => Header.Metadata.Where(m => m.Type.Name == Constants.DataPipelinePartStaticName).ToList();

        public QueryDefinition(IEntity header)
        {
            Header = header;
        }
    }
}
