using System.Reflection;

namespace ToSic.Eav.Apps.Inspect.Sys.WebApi;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWebApiControllerAssemblyLoader
{
    Assembly GetAssembly(string path);
}