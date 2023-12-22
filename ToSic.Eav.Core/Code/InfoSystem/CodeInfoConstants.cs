using ToSic.Lib.Logging;

namespace ToSic.Eav.Code.InfoSystem;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class CodeInfoConstants
{
    public const string ObsoleteNameInHistory = LogConstants.StoreWarningsPrefix + "obsolete";
    public const int MaxGeneralToLog = 25;
    public const int MaxSpecificToLog = 1;
    public const string MainError = "error";
}