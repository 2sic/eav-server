using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ToSic.Sys.Services;

/// <summary>
/// Main base class for most services which have Logs.
/// </summary>
/// <remarks>
/// This is quite internal but used heavily.
/// If you are creating DLLs using 2sxc, this may be a good choice, just remember to check if this changes in future versions.
///
/// Notes
/// 
/// * Obsolete logging-connection signatures remain as no-ops for compatibility.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
// #NoEditorBrowsableBecauseOfInheritance - would cause side-effects on inheriting classes, and would never show up when you need it to inherit from it
//[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public abstract class ServiceBase(string logName) : IHasLog
{
    /// <summary>
    /// Main constructor.
    /// </summary>
    /// <param name="logName">The name to use in the log, like "My.Purpose"</param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="connect">Obsolete compatibility parameter; ignored.</param>
    // ReSharper disable once UnusedParameter.Local
    protected ServiceBase(string logName, NoParamOrder npo = default, object[]? connect = default) : this(logName)
    { }

    /// <inheritdoc />
    [JsonIgnore]
    [IgnoreDataMember]
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public ILog Log { get; } = new Log(logName);

    /// <summary>
    /// Obsolete compatibility method; ignored.
    /// </summary>
    /// <param name="services">
    /// Former logging dependencies; ignored.
    /// </param>
    protected void ConnectLogs(object[] services)
    { }
}
