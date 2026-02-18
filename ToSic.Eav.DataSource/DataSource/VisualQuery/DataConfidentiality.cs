namespace ToSic.Eav.DataSource.VisualQuery;

/// <summary>
/// Specifies the confidentiality level of data for access control and classification purposes.
/// </summary>
/// <remarks>
/// Use this enumeration to indicate how sensitive a particular piece of data is, which can help
/// determine appropriate access restrictions and handling requirements. The values range from unknown or public data to
/// highly sensitive system-level data. Selecting the correct confidentiality level is important for enforcing security
/// policies and regulatory compliance.
///
/// By default, it's set to <see cref="Unknown"/>, indicating that the confidentiality level has not been specified.
/// 
/// Certain processes which access **any DataSource** will treat <see cref="Unknown"/> as equivalent to <see cref="System"/>
/// to avoid unintentional data exposure. Therefore, it's recommended to explicitly set the confidentiality level
/// for DataSources which handle internal information.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public enum DataConfidentiality
{
    /// <summary>
    /// Unknown / not specified; default.
    /// </summary>
    Unknown,

    /// <summary>
    /// Public data; accessible to everyone.
    /// </summary>
    Public,

    /// <summary>
    /// Internal data; accessible within the organization if someone has a login.
    /// </summary>
    Internal,

    /// <summary>
    /// Confidential data; accessible only to specific users / SiteAdmins.
    /// </summary>
    Confidential,

    /// <summary>
    /// Secret data; highly sensitive and restricted.
    /// </summary>
    Secret,

    /// <summary>
    /// System data; accessible only to system processes and host users.
    /// This kind of data may contain critical system information leading to privilege escalation.
    /// </summary>
    System,

    /// <summary>
    /// Specifies that this data should never be made available, not even to the system administrators.
    /// </summary>
    Never,
}
