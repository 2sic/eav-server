using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IDeferredEntitiesList
    {
        IEnumerable<IEntity> Entities { get; }  

        IMetadataProvider Metadata { get; }
    }
}
