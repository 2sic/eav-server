
namespace ToSic.Eav.Internal.Environment;

/// <summary>
/// Goal is that anything on this will be able to provide HttpContext operations as needed
/// To abstract .net451 and .net core
/// </summary>
public interface IServerPaths
{
    /// <summary>
    /// Get the full path of an app. Depending on the environment this can be in different roots.
    /// For example, in DNN it's in the normal website, but in Oqtane it's in the special Content. 
    /// </summary>
    /// <param name="virtualPath">The full, relative path to the app.</param>
    /// <returns>Full path beginning with drive letter (like c:) or network path (//...)</returns>
    string FullAppPath(string virtualPath);

    ///// <summary>
    ///// Get the full path of a system file. Depending on the environment this can be in different roots.
    ///// </summary>
    ///// <param name="virtualPath">The full, relative path to the system file.</param>
    ///// <returns>Full path beginning with drive letter (like c:) or network path (//...)</returns>
    //string FullSystemPath(string virtualPath);

    string FullContentPath(string virtualPath);

    string FullPathOfReference(string fileReference);
}