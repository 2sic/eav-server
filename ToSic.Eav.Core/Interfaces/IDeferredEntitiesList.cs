using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Interfaces
{
    public interface IDeferredEntitiesList: ICacheExpiring
    {
        IEnumerable<IEntity> List { get; }  

        IMetadataProvider Metadata { get; }
    }
}
