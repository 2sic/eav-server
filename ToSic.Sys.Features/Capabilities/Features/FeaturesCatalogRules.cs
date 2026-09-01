namespace ToSic.Sys.Capabilities.Features;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class FeaturesCatalogRules
{
    public static FeatureSecurity Security0Improved = new(0, "Actually increases security.");
    public static FeatureSecurity Security0Neutral = new(0, "Should not affect security.");
}