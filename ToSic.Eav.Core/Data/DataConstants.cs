namespace ToSic.Eav.Data;

/// <summary>
/// Constants for general data creation, manipulation etc.
///
/// Note that even though you may expect constants, these are usually static variables.
/// This is to ensure that they don't get compiled into other DLLs, but will always be retrieved from here.
/// </summary>
public class DataConstants
{
    #region DataFactory Constants

    public static readonly int DataFactoryDefaultAppId = 0;

    public static readonly int DataFactoryDefaultEntityId = 0;

    public static readonly string DataFactoryDefaultTypeName = "unspecified";

    public const int DataFactoryDefaultIdSeed = 1;

    #endregion

    #region Error Constants

    public static readonly int ErrorAppId = 0;
    public static readonly int ErrorEntityId = 0;
    public static readonly string ErrorTypeName = "Error";
    public static readonly string ErrorFieldTitle = "Error";
    public static readonly string ErrorFieldMessage = "Message";
    public static readonly string ErrorFieldDebugNotes = "DebugNotes";

    public static readonly string ErrorDebugMessage =
        "There should be more details in the insights logs, see https://go.2sxc.org/insights";

    #endregion
}