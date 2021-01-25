#if NETFRAMEWORK
using System;
using ToSic.Eav.Documentation;

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

        // 2020-11-08 2dm - I believe this was never publicly used
        // so now that it's obsolete, I'll first just comment it out
        ///// <summary>
        ///// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
        ///// Automatically resolves the language-variations as well based on the list of preferred languages.
        ///// Will cast/convert to the expected type, or return null / default value for that type if not possible.
        ///// </summary>
        ///// <param name="name">Name of the attribute or virtual attribute</param>
        ///// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
        ///// <returns>
        ///// An object OR a null - for example when retrieving the title and no title exists
        ///// the object is string, int or even a EntityRelationship
        ///// </returns>
        //TVal GetBestValue<TVal>(string name, bool resolveHyperlinks/* = false*/);
    }
}
#endif