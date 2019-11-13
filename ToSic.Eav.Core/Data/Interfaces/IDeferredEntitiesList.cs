using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data
{
    // todo: rename to better say what it's for, 
    // todo: decide where to put this in the final destination
    public interface IDeferredEntitiesList: ICacheExpiring
    {
        IEnumerable<IEntity> List { get; }  


        IMetadataProvider Metadata { get; }
    }
}
