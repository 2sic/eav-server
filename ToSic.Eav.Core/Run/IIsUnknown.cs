using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// Marks un-implemented objects like SiteUnknown etc.
    /// This will often reduce functionality if such objects are found
    /// </summary>
    [PrivateApi]
    public interface IIsUnknown
    {
    }
}
