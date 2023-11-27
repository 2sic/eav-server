﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services;

/// <summary>
/// Main base class for most services and helpers which have Logs.
/// Also has an API to auto-connect the logs of child-services.
/// </summary>
[PrivateApi]
public abstract class ServiceBase: IHasLog
{
    [PrivateApi]
    protected ServiceBase(string logName) => Log = new Log(logName);

    /// <inheritdoc />
    [JsonIgnore]
    [IgnoreDataMember]
    public ILog Log { get; }

    /// <summary>
    /// Connect Log of all dependencies listed in <see cref="services"/>
    /// </summary>
    /// <param name="services">One or more services which could implement <see cref="ILazyInitLog"/> or <see cref="IHasLog"/></param>
    protected void ConnectServices(params object[] services) => (this as IHasLog).ConnectServices(services);
}