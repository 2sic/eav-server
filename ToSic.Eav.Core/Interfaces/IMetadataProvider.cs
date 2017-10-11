using System;
using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// For accessing metadata from the data source. Mainly used in the Store and Cache-Systems, others probably cannot implement it 
    /// because the supply-pipeline won't have everything needed. 
    /// </summary>
    public interface IMetadataProvider
    {
        /// <summary>
        /// Get assigned Entities for specified assignmentObjectTypeId and key
        /// </summary>
        IEnumerable<IEntity> GetMetadata(int targetType, int key, string contentTypeName = null);

        /// <summary>
        /// Get assigned Entities for specified assignmentObjectTypeId and key
        /// </summary>
        IEnumerable<IEntity> GetMetadata(int targetType, string key, string contentTypeName = null);

        /// <summary>
        /// Get assigned Entities for specified assignmentObjectTypeId and key
        /// </summary>
        IEnumerable<IEntity> GetMetadata(int targetType, Guid key, string contentTypeName = null);

        //int GetMetadataType(string typeName);

        //string GetMetadataType(int typeId);

    }
}
