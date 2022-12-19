#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Eav.Metadata;
using ToSic.Eav.Security;

// This is old stuff / compatibility necessary for DNN
// It should not bleed into Oqtane or newer implementations

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// The primary data-item in the system, IEntity is a generic data-item for any kind of information.
    /// Note that it inherits <see cref="IEntityLight"/> which is the basic definition without languages,
    /// versioning, publishing etc.
    /// </summary>
    [PrivateApi]
    public partial interface IEntity: IEntityLight, 
        IPublish<Data.IEntity>, // needed to disable this for compatibility with entities - but must be typed to the new interface
        IHasPermissions
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
        T GetBestValue<T>(string attributeName, string[] languages);


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
        IMetadataOf Metadata { get; }

        #region Children & Parents
        [PrivateApi]
        List<Data.IEntity> Children(string field = null, string type = null);
        [PrivateApi]
        List<Data.IEntity> Parents(string type = null, string field = null);

        #endregion
        
    }
}
#endif