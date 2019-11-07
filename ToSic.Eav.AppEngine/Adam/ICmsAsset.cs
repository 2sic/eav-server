﻿using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Adam
{
    /// <summary>
    /// Describes an ADAM (Automatic Digital Asset Management) asset. <br/>
    /// This contains properties which both <see cref="IFolder"/> and <see cref="IFile"/> have in common.
    /// </summary>
    [PublicApi]
    public interface ICmsAsset
    {
        #region Metadata
        /// <summary>
        /// Informs the code if this asset has real metadata attached or not. 
        /// </summary>
        /// <returns>True if this asset has metadata, false if it doesn't (in which case the Metadata property still works, but won't deliver any real values)</returns>
         bool HasMetadata { get; }

        /// <summary>
        /// List of metadata items - 
        /// will automatically contain a fake item, even if no metadata exits
        /// to help in razor template etc.
        /// </summary>
        /// <returns>An IDynamicEntity which contains the metadata, or an empty IDynamicEntity which still works if no metadata exists.</returns>
         dynamic Metadata { get; }
        #endregion


        /// <summary>
        /// The path to this asset as used from external access
        /// </summary>
        /// <returns>The url to this asset</returns>
         string Url { get; }

        /// <summary>
        /// The type of this asset (folder, file, etc.)
        /// </summary>
        /// <returns>"folder", "image", "document", "file" depending on what it is</returns>
         string Type { get; }

    };
}

