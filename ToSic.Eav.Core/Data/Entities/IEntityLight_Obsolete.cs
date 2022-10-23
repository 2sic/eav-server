#if NETFRAMEWORK
using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial interface IEntityLight
    {
		[PrivateApi("Testing / wip #IValueConverter")]
        TVal GetBestValue<TVal>(string name);

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

        /// <summary>
        /// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        /// Automatically resolves the language-variations as well based on the list of preferred languages
        /// </summary>
        /// <param name="attributeName">Name of the attribute or virtual attribute</param>
        /// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
        /// <returns>
        /// An object OR a null - for example when retrieving the title and no title exists
        /// the object is string, int or even a EntityRelationship
        /// </returns>
        [Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
        [PrivateApi]
        object GetBestValue(string attributeName, bool resolveHyperlinks);

    }
}
#endif