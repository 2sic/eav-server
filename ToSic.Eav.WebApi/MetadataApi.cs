﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Conversion;
using ToSic.Eav.Plumbing;
using ToSic.Eav.WebApi.Helpers;
using IEntity = ToSic.Eav.Data.IEntity;
using ToSic;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Web API Controller for MetaData
	/// Metadata-entities (content-items) are additional information about some other object
	/// </summary>
	public static class MetadataApi
    {
        /// <summary>
        /// Get Entities with specified AssignmentObjectTypeId and Key
        /// </summary>
        public static IEnumerable<Dictionary<string, object>> Get(IServiceProvider serviceProvider, int appId, int targetType, string keyType, string key, string contentType = null)
        {
            IEnumerable<IEntity> entityList = null;

            var appState = State.Get(appId);

            switch (keyType)
            {
                case "guid":
                    if(Guid.TryParse(key, out var guidKey))
                        entityList = appState.GetMetadata(targetType, guidKey, contentType);
                    break;
                case "string":
                    entityList = appState.GetMetadata(targetType, key, contentType);
                    break;
                case "number":
                    if(int.TryParse(key, out var keyInt))
                        entityList = appState.GetMetadata(targetType, keyInt, contentType);
                    break;
                default:
                    throw new Exception("key type unknown:" + keyType);
            }

            if(entityList == null)
                throw new Exception($"was not able to convert '{key}' to key-type {keyType}, must cancel");

            return serviceProvider.Build<EntitiesToDictionary>().EnableGuids().Convert(entityList);
        }

    }
}