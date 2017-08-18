using System;
using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IMetadataProvider
    {
        IEnumerable<IEntity> GetMetadata(int targetType, int key, string contentTypeName = null);

        IEnumerable<IEntity> GetMetadata(int targetType, string key, string contentTypeName = null);

        IEnumerable<IEntity> GetMetadata(int targetType, Guid key, string contentTypeName = null);

    }
}
