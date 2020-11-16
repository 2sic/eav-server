using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// The primary data-item in the system, IEntity is a generic data-item for any kind of information.
    /// Note that it inherits <see cref="IEntityLight"/> which is the basic definition without languages,
    /// versioning, publishing etc.
    /// > We recommend you read about the @Specs.Data.Intro
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public partial interface IEntity: IEntityLight, IPublish<IEntity>, IHasPermissions
    {
        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="languages">list of languages to search in</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
#if NET451
        new 
#endif
            object GetBestValue(string attributeName, string[] languages);

        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages.
        /// Will cast/convert to the expected type, or return null / default value for that type if not possible.
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="languages">list of languages to search in</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
#if NET451
        new
#endif
            T GetBestValue<T>(string attributeName, string[] languages);

        [WorkInProgressApi("Still wip")]
        object PrimaryValue(string attributeName);

        [WorkInProgressApi("Still wip")]
        T PrimaryValue<T>(string attributeName);

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <param name="dimensions">Array of dimensions/languages to use in the lookup</param>
        /// <returns>The entity title as a string</returns>
#if NET451
        new
#endif
            string GetBestTitle(string[] dimensions);

        /// <summary>
        /// All the attributes of the current Entity.
        /// </summary>
#if NET451
        new
#endif
            Dictionary<string, IAttribute> Attributes { get; }

        /// <summary>
        /// Gets the "official" Title-Attribute <see cref="IAttribute{T}"/>
        /// </summary>
        /// <returns>
        /// The title of this Entity.
        /// The field used is determined in the <see cref="IContentType"/>.
        /// If you need a string, use GetBestTitle() instead.
        /// </returns>
        new IAttribute Title { get; }

        /// <summary>
        /// Gets an Attribute using its StaticName
        /// </summary>
        /// <param name="attributeName">StaticName of the Attribute</param>
        /// <returns>A typed Attribute Object</returns>
        new IAttribute this[string attributeName] { get; }

        /// <summary>
        /// version of this entity in the repository
        /// </summary>
        /// <returns>The version number.</returns>
#if NET451
        new
#endif
            int Version { get; }


        /// <summary>
        /// Get the metadata for this item
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        /// <returns>A typed Metadata provider for this Entity</returns>
#if NET451
        new
#endif
            IMetadataOf Metadata { get; }

        #region Children & Parents

        /// <summary>
        /// Get all the children <see cref="IEntity"/> items - optionally only of a specific field and/or type
        /// </summary>
        /// <param name="field">Optional field name to access</param>
        /// <param name="type">Optional type to filter for</param>
        /// <returns>List of children, or empty list if not found</returns>
#if NET451
        new
#endif
            List<IEntity> Children(string field = null, string type = null);

        /// <summary>
        /// Get all the parent <see cref="IEntity"/> items - optionally only of a specific type and/or referenced in a specific field
        /// </summary>
        /// <param name="type">The type name to filter for</param>
        /// <param name="field">The field name where a parent references this item</param>
        /// <returns>List of children, or empty list if not found</returns>
#if NET451
        new
#endif
            List<IEntity> Parents(string type = null, string field = null);

        [PrivateApi]
        object Value(string field);
        [PrivateApi]
        T Value<T>(string field);

        #endregion
    }
}
