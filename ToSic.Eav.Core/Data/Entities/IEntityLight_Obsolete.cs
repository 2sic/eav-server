﻿#if NETFRAMEWORK
namespace ToSic.Eav.Data;

partial interface IEntityLight
{
    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
	//[PrivateApi("Testing / wip #IValueConverter")]
    //TVal GetBestValue<TVal>(string name);

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <summary>
    ///// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
    ///// Automatically resolves the language-variations as well based on the list of preferred languages
    ///// </summary>
    ///// <param name="attributeName">Name of the attribute or virtual attribute</param>
    ///// <returns>
    ///// An object OR a null - for example when retrieving the title and no title exists
    ///// the object is string, int or even a EntityRelationship
    ///// </returns>
    //[Obsolete("Deprecated. Do not use any more, as it cannot reliably know the real language list. Use GetBestValue(name, languageList)")]
    //[PrivateApi]
    //object GetBestValue(string attributeName);

    // #EntityLight-UnusedAttributes - turned off 2025-01-17 2dm, probably remove 2025-Q2
    ///// <summary>
    ///// Retrieves the best possible value for an attribute or virtual attribute (like EntityTitle)
    ///// Automatically resolves the language-variations as well based on the list of preferred languages
    ///// </summary>
    ///// <param name="attributeName">Name of the attribute or virtual attribute</param>
    ///// <param name="resolveHyperlinks">If true, will try to resolve links in the value. Default is false.</param>
    ///// <returns>
    ///// An object OR a null - for example when retrieving the title and no title exists
    ///// the object is string, int or even a EntityRelationship
    ///// </returns>
    //[Obsolete("Deprecated. Do not use any more, as it cannot reliably resolve hyperlinks.")]
    //[PrivateApi]
    //object GetBestValue(string attributeName, bool resolveHyperlinks);

}

#endif