using System;

namespace ToSic.Eav.Internal.Features;

public static class EavFeaturesServiceExtensions
{
    public static void ThrowIfNotEnabled(this IEavFeaturesService featSer, string message, params Guid[] featureGuid)
    {
        if (featSer.IsEnabled(featureGuid, message, out var exception)) return;
        throw exception;
    }
}