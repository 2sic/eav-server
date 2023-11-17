using System;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
namespace ToSic.Eav.Plumbing
{
    public abstract class AppDomainHandling
    {
        public AppDomain CustomAppDomain;
        public const string ProxyType = "ToSic.Eav.Plumbing.AssemblyLoader";

        public AppDomain CreateNewAppDomain(string appDomain)
        {
            var domainSetup = new AppDomainSetup
            {
                ApplicationName = appDomain,
                ApplicationBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin") // current bin directory
            };
            return AppDomain.CreateDomain(appDomain, null, domainSetup);
        }

        public void Unload()
        {
            if (CustomAppDomain == null) return;
            AppDomain.Unload(CustomAppDomain);
            CustomAppDomain = null;
        }

        public static Type FindTypeInCurrentDomain(string typeName)
            => AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(typeName, false)).FirstOrDefault(type => type != null);
    }
}
#endif
