using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonServiceInternalUnknown : IJsonServiceInternal
{
    public JsonServiceInternalUnknown(WarnUseOfUnknown<JsonServiceInternalUnknown> _) { }

    public T To<T>(string json) => throw new NotImplementedException();
}