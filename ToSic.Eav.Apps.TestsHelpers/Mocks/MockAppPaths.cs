namespace ToSic.Eav.Apps.Mocks;

/// <summary>
/// Configurable app paths for tests that need deterministic physical and virtual app roots.
/// </summary>
public class MockAppPaths(string physicalPath) : IAppPaths
{
    public string Path { get; init; } = "/";
    public string PhysicalPath { get; init; } = physicalPath;
    public string PathShared { get; init; } = "/";
    public string PhysicalPathShared { get; init; } = physicalPath;
    public string RelativePath { get; init; } = "/";
    public string RelativePathShared { get; init; } = "/";
}
