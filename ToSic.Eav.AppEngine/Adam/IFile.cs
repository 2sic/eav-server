using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps.Adam
{
    /// <summary>
    /// An ADAM (Automatic Digital Asset Management) file
    /// </summary>
    [PublicApi]
    
    public interface IFile: 
        ICmsAsset, 
        Assets.IFile,         
        Sxc.Adam.IFile // for compatibility with old code
    {

        // internal note: this is just here for compatibility, because it's both in IFile and the old Sxc.Adam.IFile
        // but it shouldn't appear in the documentation, as it's just some hack to make sure old/new are compatible
        [PrivateApi]
        new string FullName { get; set; }

    }
}
