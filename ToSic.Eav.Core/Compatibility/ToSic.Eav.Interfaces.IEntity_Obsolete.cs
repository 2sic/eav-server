using System;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

// This is old stuff / compatibility necessary for DNN
// It should not bleed into Oqtane or newer implementations
#if NETFRAMEWORK

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// The primary data-item in the system, IEntity is a generic data-item for any kind of information.
    /// Note that it inherits <see cref="IEntityLight"/> which is the basic definition without languages,
    /// versioning, publishing etc.
    /// </summary>
    [PrivateApi]
    public partial interface IEntity
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
        [Obsolete]
        object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks);

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
        [Obsolete]
        T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks);


        // 2020-12-15 Deprecated this
        [PrivateApi]
        [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
        object PrimaryValue(string attributeName);

        // 2020-12-15 Deprecated this
        [PrivateApi]
        [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
        T PrimaryValue<T>(string attributeName);

        // 2020-12-15 Deprecated this
        [PrivateApi]
        [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
        object Value(string field, bool resolve = true);

        // 2020-12-15 Deprecated this
        [PrivateApi]
        [Obsolete("was probably never in use anywhere, but we'll leave it alive to not break existing code")]
        T Value<T>(string field, bool resolve = true);

    }
}
#endif