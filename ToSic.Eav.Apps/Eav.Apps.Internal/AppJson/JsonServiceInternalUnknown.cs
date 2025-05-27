using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class JsonServiceInternalUnknown(WarnUseOfUnknown<JsonServiceInternalUnknown> _) : IJsonServiceInternal
{

    public T To<T>(string json) => throw new NotImplementedException();
}