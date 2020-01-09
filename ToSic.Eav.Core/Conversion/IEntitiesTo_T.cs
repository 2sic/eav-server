using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// Convert an entity into another format
    /// </summary>
    /// <typeparam name="T">The target format we'll convert into</typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IEntitiesTo<out T>
    {
        /// <summary>
        /// Include the entity Guid in the conversion
        /// </summary>
        bool WithGuid { get; set; }

        /// <summary>
        /// Include publishing information (draft etc.) in the conversion
        /// </summary>
        bool WithPublishing { get; }

        /// <summary>
        /// Include metadata for-information
        /// </summary>
        bool WithMetadataFor { get; }

        /// <summary>
        /// Include the title in a special field _Title
        /// </summary>
        bool WithTitle { get; }

        /// <summary>
        /// Languages to prefer when looking up the values
        /// </summary>
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