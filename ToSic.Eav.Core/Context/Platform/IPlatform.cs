namespace ToSic.Eav.Context;

[PrivateApi("internal functionality")]
public interface IPlatformInfo
{
    string Name { get; }
    Version Version { get; }
    string Identity { get; }
}