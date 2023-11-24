using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

[PrivateApi]
public static class LookUpConstants
{
    /// <summary>
    /// This marks a special LookUp provider which is passed around through the system
    /// As of now, this special source only offers two properties
    /// * a special property which is not a lookup but contains the Instance object which is sometimes needed passed on
    /// * a lookup value ShowDrafts which informs if the user may see drafts or not
    /// </summary>
    // todo: rename sometime to be more generic
    public const string InstanceContext = "SxcInstance";
}