using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Assets
{
    /// <summary>
    /// Describes a folder as provided by the underlying environment (like DNN)
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IFolder: IAsset
    {
        /// <summary>
        /// Information if this folder has things inside it - other folders, files etc.
        /// </summary>
        /// <returns>true if it has items inside it, false if not</returns>
        bool HasChildren { get; set; }

    }
}