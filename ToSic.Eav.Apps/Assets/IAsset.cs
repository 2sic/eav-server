using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// Any asset (file/folder) of the EAV App System. <br/>
    /// This interface contains properties which both <see cref="IFolder"/> and <see cref="IFile"/> have in common
    /// </summary>
    [PublicApi]
    public interface IAsset
    {
        /// <summary>
        /// The creation date of the item, as reported by the environment. 
        /// </summary>
        /// <returns>The date-time when the file was created.</returns>
        DateTime Created { get; }

        /// <summary>
        /// The ID of the item, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        int Id { get; }

        /// <summary>
        /// The folder ID of the file, or parent-folder of a folder, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        // ReSharper disable once UnusedMemberInSuper.Global
        int ParentId { get; }

        /// <summary>
        /// The modified date of the file, as reported by the environment.
        /// </summary>
        /// <returns>The date-time when the file was modified last.</returns>
        DateTime Modified { get; }

        /// <summary>
        /// The asset name
        /// typically the folder or the file name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The _relative_ physical path of the item in the file system of the environment.
        /// This is usually relative to the site root.
        /// </summary>
        /// <returns>The full path of this item</returns>
        string Path { get; }

        /// <summary>
        /// The _full_ physical path to folder or file to access them on the local server.
        /// </summary>
        /// <returns>The full physical path to this asset</returns>
        string PhysicalPath { get; }

    }
}
