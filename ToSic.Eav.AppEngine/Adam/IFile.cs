using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Adam
{
    /// <summary>
    /// An ADAM (Automatic Digital Asset Management) file
    /// </summary>
    [PublicApi]
    
    public interface IFile: ICmsAsset, Assets.IFile
    {
    }
}
