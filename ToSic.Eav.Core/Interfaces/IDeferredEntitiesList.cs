using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IDeferredEntitiesList
    {
        IDictionary<int, IEntity> List { get; }
        IEnumerable<IEntity> LightList { get; }  

        IMetadataProvider Metadata { get; }
    }
}
