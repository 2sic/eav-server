namespace ToSic.Sys.Capabilities.Features;

[ShowApiWhenReleased(ShowApiMode.Never)]
public enum FeatureDisabledBehavior
{
    Disable,
    Downgrade,
    Warn,
    Nag,
}