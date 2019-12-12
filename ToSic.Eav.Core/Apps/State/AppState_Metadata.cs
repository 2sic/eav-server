using System.Collections.Generic;
using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    public partial class AppState: IMetadataSource
	{
        internal AppMetadataManager Metadata { get; set; }

        /// <inheritdoc />
	    public IEnumerable<IEntity> Get<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
            => Metadata.Get(targetType, key, contentTypeName);


    }
}