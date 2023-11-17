#if NETFRAMEWORK
namespace ToSic.Eav.Plumbing
{
    public class CSharpAssemblyHandling : AppDomainHandling, ICSharpAssemblyHandling
    {
        public string GetLanguageVersions()
        {
            CustomAppDomain = CreateNewAppDomain("CSharpAssemblyAppDomain"); /*AppDomain.CreateDomain("child");*/
            var proxyType = typeof(CSharpAssemblyLoader);
            var loader = (CSharpAssemblyLoader)CustomAppDomain.CreateInstanceAndUnwrap(proxyType.Assembly.FullName, proxyType.FullName);
            var result = loader.GetLanguageVersions();

            Unload();

            return result;
        }
    }
}
#endif