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
        // 2017-11-01 2dm - moving GetMetadata to a generic interface, instead of many overloads
        ///// <summary>
        ///// Get assigned Entities for specified assignmentObjectTypeId and key
        ///// </summary>
        //IEnumerable<IEntity> GetMetadata(int targetType, int key, string contentTypeName = null);

        ///// <summary>
        ///// Get assigned Entities for specified assignmentObjectTypeId and key
        ///// </summary>
        //IEnumerable<IEntity> GetMetadata(int targetType, string key, string contentTypeName = null);

        ///// <summary>
        ///// Get assigned Entities for specified assignmentObjectTypeId and key
        ///// </summary>
        //IEnumerable<IEntity> GetMetadata(int targetType, Guid key, string contentTypeName = null);


        IEnumerable<IEntity> GetMetadata<T>(int targetType, T key, string contentTypeName = null);

    }
}
