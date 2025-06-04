using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.Internal.Features;

public static class SysFeaturesServiceExtensions
{
    public static void ThrowIfNotEnabled(this ISysFeaturesService featSer, string message, params Guid[] featureGuid)
    {
        if (featSer.IsEnabled(featureGuid, message, out var exception)) return;
        throw exception;
    }
}