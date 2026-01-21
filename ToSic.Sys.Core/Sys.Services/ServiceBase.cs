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
/// * Also has an API to auto-connect the logs of child-services.
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
    /// <param name="connect">List of services to auto-connect to the logging.</param>
    // ReSharper disable once UnusedParameter.Local
    protected ServiceBase(string logName, NoParamOrder npo = default, object[]? connect = default) : this(logName)
    {
        if (connect == null)
            return;
        ConnectLogs(connect);
    }

    /// <inheritdoc />
    [JsonIgnore]
    [IgnoreDataMember]
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public ILog Log { get; } = new Log(logName);

    /// <summary>
    /// Connect Log of all dependencies listed in the `services` array.
    /// </summary>
    /// <param name="services">
    /// One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/>
    /// </param>
    protected void ConnectLogs(object[] services)
        => (this as IHasLog).ConnectLogs(services);
}