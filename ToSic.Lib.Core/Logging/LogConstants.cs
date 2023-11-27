using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

[InternalApi_DoNotUse_MayChangeWithoutNotice]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogConstants
{
    /// <summary>
    /// Max length of the scope (prefix) in the name.
    /// </summary>
    public const int ScopeMaxLength = 3;

    /// <summary>
    /// Max length of the name part after the scope prefix.
    /// </summary>
    public const int NameMaxLength = 6;

    /// <summary>
    /// Name to use in situations where the name wasn't provided.
    /// </summary>
    // ReSharper disable once StringLiteralTypo
    public const string NameUnknown = "unknwn";


    public const string FullNameUnknown = LogScopes.Unknown + "." + NameUnknown;

    /// <summary>
    /// Special prefix in the log history to ensure warnings can be extracted easily to show in a consolidated way.
    /// </summary>
    public const string StoreWarningsPrefix = "warnings-";

    /// <summary>
    /// Size of a segment in the log history.
    /// </summary>
    public const int LiveStoreSegmentSize = 100;

    public const int LiveStoreMaxItems = 500;

    public const string WarningPrefix = "WARNING: ";

    public const string ErrorPrefix = "ERROR: ";
}