using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Documentation;
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


        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        /// <param name="metadataTypes"></param>
        [PrivateApi("internal use only")]
        internal void InitMetadata(ImmutableDictionary<int, string> metadataTypes)
        {
            if (!Loading)
                throw new Exception("trying to init metadata, but not in loading state. set that first!");
            Metadata = _appTypesFromRepository == null
                ? new AppMetadataManager(this, metadataTypes, Log)
                : throw new Exception("can't set metadata if content-types are already set");

            AppMetadata = new MetadataOf<int>(Constants.MetadataForApp, AppId, this);
        }

        [PrivateApi] public IMetadataOf AppMetadata { get; private set; }
    }
}