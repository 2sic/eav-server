using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security.Permissions;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// The primary data-item in the system, IEntity is a generic data-item for any kind of information.
    /// Note that it inherits <see cref="IEntityLight"/> which is the basic definition without languages,
    /// versioning, publishing etc.
    /// </summary>
    [PrivateApi]
    public interface IEntity: IEntityLight, IPublish<IEntity>, IHasPermissions
    {
        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="languages">list of languages to search in</param>
        /// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks = false);

        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages.
        /// Will cast/convert to the expected type, or return null / default value for that type if not possible.
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="languages">list of languages to search in</param>
        /// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
        T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks = false);

        [PrivateApi]
        object PrimaryValue(string attributeName, bool resolveHyperlinks = false);
        [PrivateApi]
        T PrimaryValue<T>(string attributeName, bool resolveHyperlinks = false);

        /// <summary>
        /// Best way to get the current entities title
        /// </summary>
        /// <param name="dimensions">Array of dimensions/languages to use in the lookup</param>
        /// <returns>The entity title as a string</returns>
        string GetBestTitle(string[] dimensions);

        /// <summary>
        /// All the attributes of the current Entity.
        /// </summary>
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
        int Version { get; }


        /// <summary>
        /// Get the metadata for this item
        /// </summary>
        /// <remarks>
        /// The metadata is either already prepared, from the same app, or from a remote app
        /// </remarks>
        /// <returns>A typed Metadata provider for this Entity</returns>
        IMetadataOfItem Metadata { get; }

        #region experimental IEntity Queryable / Quick
        [PrivateApi]
        List<IEntity> Children(string field = null, string type = null);
        [PrivateApi]
        List<IEntity> Parents(string type = null, string field = null);

        [PrivateApi]
        object Value(string field, bool resolve = true);
        [PrivateApi]
        T Value<T>(string field, bool resolve = true);

        #endregion experimental
    }
}
