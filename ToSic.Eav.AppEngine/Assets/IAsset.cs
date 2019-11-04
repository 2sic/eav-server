﻿using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// Any asset (file/folder) of the EAV App System. <br/>
    /// This interface contains properties which both files and folders have in common
    /// </summary>
    [PublicApi]
    public interface IAsset
    {
        /// <summary>
        /// The creation date of the item, as reported by the environment. 
        /// </summary>
        /// <returns>The date-time when the file was created.</returns>
        DateTime Created { get; set; }

        /// <summary>
        /// The ID of the item, if the underlying environment uses int IDs
        /// </summary>
        /// <returns>an int with the id used by the environment to track this item</returns>
        int Id { get; set; }

        /// <summary>
        /// The modified date of the file, as reported by the environment.
        /// </summary>
        /// <returns>The date-time when the file was modified last.</returns>
        DateTime Modified { get; set; }

        /// <summary>
        /// The asset name
        /// typically the folder or the file name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The path of the item in the file system of the environment.
        /// </summary>
        /// <returns>The full path of this item</returns>
        string Path { get; set; }

    }
}
