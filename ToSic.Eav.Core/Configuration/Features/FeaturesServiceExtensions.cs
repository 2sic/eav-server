using System;

namespace ToSic.Eav.Configuration
{
    public static class FeaturesServiceExtensions
    {
        public static void ThrowIfNotEnabled(this IFeaturesInternal featSer, string message, params Guid[] featureGuid)
        {
            if (featSer.IsEnabled(featureGuid, message, out var exception)) return;
            throw exception;
        }
    }
}
