﻿using System.Collections.Generic;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    public interface IEntityTo<out T>
    {
        /// <summary>
        /// Include the entity Guid in the conversion
        /// </summary>
        bool IncludeGuid { get; set; }

        /// <summary>
        /// Include publishing information (draft etc.) in the conversion
        /// </summary>
        bool IncludePublishingInfo { get; }

        /// <summary>
        /// Include metadata for-information
        /// </summary>
        bool IncludeMetadataFor { get; }

        /// <summary>
        /// Include the title in a special field _Title
        /// </summary>
        bool ProvideIdentityTitle { get; }


        string[] Languages { get; set; }

        /// <summary>
        /// Ensure all settings are so it includes guids etc.
        /// This is so the serializable information is useful for admin UIs
        /// </summary>
        void ConfigureForAdminUse();

        /// <summary>
        /// Return an list of converted entities, ready to serialize
        /// </summary>
        /// <remarks>
        ///     note that this could be in use on webAPIs and scripts
        ///     so even if it looks un-used, it must stay available
        /// </remarks>
        IEnumerable<T> Convert(IEnumerable<IEntity> entities);

        /// <summary>
        /// Return an converted, serializable entity
        /// </summary>
        T Convert(IEntity entity);

    }
}