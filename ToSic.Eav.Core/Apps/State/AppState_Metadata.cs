﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    public partial class AppState: IMetadataSource, IHasMetadata
    {
        /// <inheritdoc />
	    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
            => _metadataManager.GetMetadata(targetType, key, contentTypeName);

        /// <inheritdoc />
        public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null)
            => _metadataManager.GetMetadata(targetType, key, contentTypeName);
        
        [PrivateApi]
        public IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title = null) => new MetadataOf<T>((int)targetType, key, this, title);


        /// <summary>
        /// The first init-command to run after creating the package
        /// it's needed, so the metadata knows what lookup types are supported
        /// </summary>
        [PrivateApi("internal use only")]
        // #removeUnusedPreloadOfMetaTypes
        public void InitMetadata(/*ImmutableDictionary<int, string> metadataTypes*/)
        {
            if (!Loading)
                throw new Exception("Trying to init metadata, but App is not in loading state.");
            if (_appContentTypesFromRepository != null)
                throw new Exception("Can't init metadata if content-types are already set");
            // #removeUnusedPreloadOfMetaTypes
            _metadataManager = new AppMetadataManager(this /*, metadataTypes*/);

            Metadata = GetMetadataOf(TargetTypes.App, AppId, "App (" + AppId + ") " + Name + " (" + Folder + ")");
        }
        private AppMetadataManager _metadataManager;

        /// <summary>Metadata describing this App</summary>
        public IMetadataOf Metadata { get; private set; }
    }
}