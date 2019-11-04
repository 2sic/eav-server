using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// An ADAM (Automatic Digital Asset Management) folder
    /// </summary>
    [PublicApi]
    public interface IAdamFolder: IFolder, IAdamAsset
    {
        /// <summary>
        /// Get the files in this folder
        /// </summary>
        /// <returns>A list of files in this folder as <see cref="IAdamFile"/></returns>
        IEnumerable<IAdamFile> Files { get; }

        /// <summary>
        /// Get the sub-folders in this folder
        /// </summary>
        /// <returns>A list of folders inside this folder - as <see cref="IAdamFolder"/>.</returns>
        IEnumerable<IAdamFolder> Folders { get; }
    }
}
