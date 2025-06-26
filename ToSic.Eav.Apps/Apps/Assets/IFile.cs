namespace ToSic.Eav.Apps.Assets;

/// <summary>
/// Describes a file asset as provided by the underlying environment (like DNN and Oqtane).
/// </summary>
/// <remarks>
/// All APIs retrieving such a file always begin in either an App or ADAM context.
/// So any relative path information begins there, as these paths are usually meant to be used either relative to that location,
/// or for generating URLs.
/// </remarks>
[PublicApi]
public interface IFile: IAsset
{
    /// <summary>
    /// The file extension of the real underlying file, without the leading dot.
    /// </summary>
    /// <returns>
    /// The extension, like "pdf" or "jpg".
    /// `pdf` for `C:\Inetpub\wwwroot\www.2sic.com\Portals\0\2sxc\content\assets\docs\terms\file.pdf`
    /// </returns>
    string Extension { get; }

    /// <summary>
    /// The full folder of the file beginning from the root (App or ADAMA), with trailing slash.
    /// </summary>
    /// <returns>
    /// The folder name.
    /// `assets/docs/terms/` for `C:\Inetpub\wwwroot\www.2sic.com\Portals\0\2sxc\content\assets\docs\terms\file.pdf`
    /// </returns>
    string Folder { get; }

    /// <summary>
    /// The folder ID of the file, if the underlying environment uses int IDs
    /// </summary>
    /// <returns>
    /// The id used by the environment to track this item.
    /// `19350` for `C:\Inetpub\wwwroot\www.2sic.com\Portals\0\2sxc\content\assets\docs\terms\file.pdf`
    /// </returns>
    int FolderId { get; }

    /// <summary>
    /// The full file name of the original file
    /// </summary>
    /// <returns>
    /// The full file name with extension.
    /// `file.pdf` for `C:\Inetpub\wwwroot\www.2sic.com\Portals\0\2sxc\content\assets\docs\terms\file.pdf`
    /// </returns>
    string FullName { get; }

    /// <summary>
    /// The file size of the file, IF the underlying environment provides this.
    /// </summary>
    /// <returns>
    /// The size in bytes.
    /// `18273` for `C:\Inetpub\wwwroot\www.2sic.com\Portals\0\2sxc\content\assets\docs\terms\file.pdf`
    /// </returns>
    int Size { get; }

    /// <summary>
    /// Size information object for files with specific properties to get the size in bytes, kilobytes, megabytes, etc.
    /// </summary>
    /// <remarks>
    /// Added in v14.04
    /// </remarks>
    ISizeInfo SizeInfo { get; }
}