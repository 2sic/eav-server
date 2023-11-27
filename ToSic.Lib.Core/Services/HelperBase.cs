using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Lib.Services;

/// <summary>
/// Main base class for most helpers which don't use Dependency Injection.
/// These are mainly classes that are SoC helpers which cover one single aspect used in another service.
/// They are not meant for DI, so the parent logger should be included in the initial call.
/// </summary>
[PrivateApi]
public abstract class HelperBase: IHasLog
{
    [PrivateApi]
    protected HelperBase(ILog parentLog, string logName) => Log = new Log(logName, parentLog);

    /// <inheritdoc />
    [JsonIgnore]
    [IgnoreDataMember]
    public ILog Log { get; }
        
}