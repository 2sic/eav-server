﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ToSic.Sys.Services;

/// <summary>
/// Main base class for most services and helpers which have Logs.
/// Also has an API to auto-connect the logs of child-services.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
// #NoEditorBrowsableBecauseOfInheritance - would cause side-effects on inheriting classes
//[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public abstract class ServiceBase(string logName) : IHasLog
{
    /// <summary>
    /// Experimental signature v17.02...
    /// </summary>
    /// <param name="logName"></param>
    /// <param name="protect"></param>
    /// <param name="connect"></param>
    // ReSharper disable once UnusedParameter.Local
    protected ServiceBase(string logName, NoParamOrder protect = default, object[]? connect = default) : this(logName)
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

    // 2025-05-19 disabled, should not be in use anymore; #remove ca. 2025-Q3
    ///// <summary>
    ///// Connect Log of all dependencies listed in <see cref="services"/>
    ///// </summary>
    ///// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
    //[Obsolete("Avoid using, will be removed soon. Use ConnectLogs([...])")]
    //protected void ConnectServices(params object[] services) => (this as IHasLog).ConnectLogs(services);

    /// <summary>
    /// Connect Log of all dependencies listed in <see cref="services"/>
    /// </summary>
    /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
    protected void ConnectLogs(object[] services)
        => (this as IHasLog).ConnectLogs(services);
}