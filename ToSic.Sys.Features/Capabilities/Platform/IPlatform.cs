namespace ToSic.Sys.Capabilities.Platform;

[PrivateApi("internal functionality")]
public interface IPlatformInfo
{
    string Name { get; }
    Version Version { get; }
    string Identity { get; }
}