using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Paths;

/// <summary>
/// Internal interface to enable helpers to switch between paths on both the App and AppPaths
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppPaths
{
    /// <summary>
    /// The path to the current app, for linking JS/CSS files and
    /// images in the app folder. 
    /// </summary>
    /// <returns>Path usually starting with /portals/...</returns>
    string Path { get; }

    /// <summary>
    /// The path on the server hard disk for the current app. 
    /// </summary>
    /// <returns>Path usually starting with c:\...</returns>
    string PhysicalPath { get; }

    /// <summary>
    /// The path to the current app global folder, for linking JS/CSS files and
    /// images in the app folder. 
    /// </summary>
    /// <returns>Path usually starting with /portals/_default/...</returns>
    /// <remarks>Added v13.01</remarks>
    string PathShared { get; }

    /// <summary>
    /// The path on the server hard disk for the current app global folder. 
    /// </summary>
    /// <returns>Path usually starting with c:\...</returns>
    /// <remarks>Added v13.01</remarks>
    string PhysicalPathShared { get; }

    /// <summary>
    /// Path relative to the website root.
    /// In DNN this is usually the same as the url-path.
    /// In Oqtane it's very different. 
    /// </summary>
    /// <remarks>
    /// * Made public v15.06 but existed previously
    /// </remarks>
    [PrivateApi("not public, not sure if we should surface this")]
    string RelativePath { get; }

    /// <summary>
    /// Path of the shared App relative to the website root.
    /// In DNN this is usually the same as the url-path.
    /// In Oqtane it's very different. 
    /// </summary>
    /// <remarks>
    /// * Made public v15.06 but existed previously
    /// </remarks>
    [PrivateApi("not public, not sure if we should surface this")]
    string RelativePathShared { get; }
}