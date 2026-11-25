namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppReadContentTypes
{
    /// <summary>
    /// All content types of the app.
    /// </summary>
    IEnumerable<IContentType> ContentTypes { get; }

    /// <summary>
    /// Get a single content type by name (display name or NameId).
    /// </summary>
    /// <param name="name">the name, either the normal name or the NameId which looks like a GUID</param>
    /// <returns></returns>
    IContentType? TryGetContentType(string name);

    /// <summary>
    /// Get a single content type by name (display name or NameId).
    /// </summary>
    /// <remarks>
    /// Will throw an exception if it fails; use TryGetContentType if you want to avoid exceptions.
    /// </remarks>
    /// <param name="name">the name, either the normal name or the NameId which looks like a GUID</param>
    /// <returns></returns>
    IContentType GetContentType(string name);

}