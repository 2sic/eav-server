﻿using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Adam
{
    /// <summary>
    /// An ADAM (Automatic Digital Asset Management) folder
    /// </summary>
    [PublicApi]
    public interface IFolder: Assets.IFolder, ICmsAsset
    {
        /// <summary>
        /// Get the files in this folder
        /// </summary>
        /// <returns>A list of files in this folder as <see cref="IFile"/></returns>
        IEnumerable<IFile> Files { get; }

        /// <summary>
        /// Get the sub-folders in this folder
        /// </summary>
        /// <returns>A list of folders inside this folder - as <see cref="IFolder"/>.</returns>
        IEnumerable<IFolder> Folders { get; }
    }
}
