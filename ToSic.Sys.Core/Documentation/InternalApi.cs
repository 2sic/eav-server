namespace ToSic.Lib.Documentation;

/// <summary>
/// This attribute serves as metadata for other things to mark them as internal APIs.
/// Use this on objects/properties/methods you want to document publicly
/// for example to improve understanding of the system - but still mark as _internal_ so people are warned
/// </summary>
[PublicApi]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public class InternalApi_DoNotUse_MayChangeWithoutNotice: Attribute
{
    public InternalApi_DoNotUse_MayChangeWithoutNotice() { }

    public InternalApi_DoNotUse_MayChangeWithoutNotice(string? comment = null) { }

}