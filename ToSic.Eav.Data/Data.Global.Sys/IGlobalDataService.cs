using System.Collections.Immutable;

namespace ToSic.Eav.Data.Global.Sys;

/// <summary>
/// Accessor for global content types, which are used for serializers.
/// </summary>
/// <remarks>
/// Since there are cases during boot where the global content types are not yet available,
/// there is both an allow-null and strict mode for this service.
/// </remarks>
public interface IGlobalDataService
{
    /// <summary>
    /// Get a single content type by name (display name or NameId).
    /// </summary>
    /// <param name="name">the name, either the normal name or the NameId which looks like a GUID</param>
    /// <returns>a type object or null if not found</returns>
    IContentType GetContentType(string name);

    /// <returns>a type object or null if not found</returns>
    IContentType GetContentTypeIfAlreadyLoaded(string name);

    IImmutableList<IEntity> ListRequired { get; }
}
