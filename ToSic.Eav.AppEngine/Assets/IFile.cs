using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// Describes a file asset as provided by the underlying environment (like DNN)
    /// </summary>
    [PublicApi]
    public interface IFile: IAsset
    {
        /// <summary>
        /// The file extension of the real underlying file.
        /// </summary>
        /// <returns>
        /// The extension, like "pdf" or "jpg"
        /// </returns>
        string Extension { get; set; }

        /// <summary>
        /// The folder of the file
        /// </summary>
        /// <returns>The folder name </returns>
        string Folder { get; set; }

        /// <summary>
        /// The folder ID of the file, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        int FolderId { get; set; }

        /// <summary>
        /// The full file name of the original file
        /// </summary>
        /// <returns>The full file name with extension.</returns>
        string FullName { get; set; }

        /// <summary>
        /// The file size of the file, IF the underlying environment provides this.
        /// </summary>
        /// <returns>the size in bytes</returns>
        int Size { get; set; }
    }
}