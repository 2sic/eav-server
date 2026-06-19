using System.Reflection;

namespace ToSic.Eav.WebApi.Sys.ApiExplorer;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWebApiControllerAssemblyLoader
{
    Assembly GetAssembly(string path);
}