using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// For accessing metadata from the data source. Mainly used in the Store and Cache-Systems, others probably cannot implement it 
    /// because the supply-pipeline won't have everything needed. 
    /// </summary>
    public interface IMetadataProvider: ICacheExpiring
    {
        IEnumerable<IEntity> GetMetadata<T>(int targetType, T key, string contentTypeName = null);

    }
}
