#if NETFRAMEWORK


// This is old stuff / compatibility necessary for DNN
// It should not bleed into Oqtane or newer implementations
namespace ToSic.Eav.Data;

partial interface IEntity: Interfaces.IEntity // compatibility - only relevant for DNN
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
    [PrivateApi]
    new object GetBestValue(string attributeName, string[] languages, bool resolveHyperlinks);

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
    [PrivateApi]
    new T GetBestValue<T>(string attributeName, string[] languages, bool resolveHyperlinks);


    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    // 2025-01-17 moved to IEntity - still obsolete, but slimming down IEntityLight
    /// <summary>
    /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
    /// Automatically resolves the language-variations as well based on the list of preferred languages
    /// </summary>
    /// <param name="attributeName">Name of the attribute or virtual attribute</param>
    /// <returns>
    /// An object OR a null - for example when retrieving the title and no title exists
    /// the object is string, int or even a EntityRelationship
    /// </returns>
    [Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
    [PrivateApi]
    object GetBestValue(string attributeName);

}

#endif
