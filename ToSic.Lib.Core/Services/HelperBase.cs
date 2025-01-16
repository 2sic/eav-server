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
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[method: PrivateApi]
public abstract class HelperBase(ILog parentLog, string logName) : IHasLog
{
    /// <inheritdoc />
    [JsonIgnore]        // Prevent System.Text.Json from serializing this property
    [IgnoreDataMember]  // Prevent Newtonsoft Json from serializing this property, without depending on the Newtonsoft.Json package
    [PrivateApi]
    public ILog Log { get; } = new Log(logName, parentLog);
}