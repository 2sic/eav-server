using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ToSic.Sys.Services;

/// <summary>
/// Main base _record_ for most helpers which don't use Dependency Injection.
/// </summary>
/// <remarks>
/// These are mainly records that are SoC helpers which cover one single aspect used in another service.
/// They are not meant for DI, so the parent logger should be included in the initial call.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record HelperRecordBase : IHasLog
{
    /// <summary>
    /// Constructor, implemented as non-primary because we don't want the parameters to become properties of the record.
    /// </summary>
    /// <param name="parentLog"></param>
    /// <param name="logName"></param>
    [PrivateApi]
    protected HelperRecordBase(ILog? parentLog, string logName)
    {
        Log = new Log(logName, parentLog);
    }

    /// <inheritdoc />
    [JsonIgnore] // Prevent System.Text.Json from serializing this property
    [IgnoreDataMember] // Prevent Newtonsoft Json from serializing this property, without depending on the Newtonsoft.Json package
    [field: AllowNull, MaybeNull]
    [PrivateApi]
    public ILog Log { get; }
}