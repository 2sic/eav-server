namespace ToSic.Sys.Capabilities.Platform;

[PrivateApi("internal functionality")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPlatformInfo
{
    string Name { get; }
    Version Version { get; }
    string Identity { get; }
}