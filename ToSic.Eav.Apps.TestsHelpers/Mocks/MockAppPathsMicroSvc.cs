using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Context;

namespace ToSic.Eav.Apps.Mocks;

/// <summary>
/// Test implementation returning the same configurable app paths for all app readers.
/// </summary>
public class MockAppPathsMicroSvc(string root) : IAppPathsMicroSvc
{
    public IAppPaths Get(IAppReader appReader) => new MockAppPaths(root);

    public IAppPaths Get(IAppReader appReader, ISite? siteOrNull) => new MockAppPaths(root);
}
