using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IDeferredEntitiesList
    {
        IEnumerable<IEntity> List { get; }  

        IMetadataProvider Metadata { get; }
    }
}
