using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Plumbing
{
    public class AssemblyHandling
    {

        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        public static IEnumerable<Type> FindInherited(Type type, ILog log = null)
        {
            log?.Add($"FindInherited of type {type.FullName}");
            return GetTypes(log).Where(t => type.IsAssignableFrom(t) && (!t.IsAbstract || t.IsInterface) && t != type);
        }

        internal static List<Type> GetTypes(ILog log = null)
        {
            if (_typeCache != null) return _typeCache;
            
            var wrapLog = log.Call2<List<Type>>(startTimer: true);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            log.SafeAdd($"GetTypes() - found {assemblies.Length} assemblies");

            _typeCache = assemblies.SelectMany(a => GetLoadableTypes(a, log)).ToList();

            return wrapLog.Return(_typeCache, $"{_typeCache.Count}");
        }

        private static List<Type> _typeCache;

        /// <summary>
        /// Get Loadable Types from an assembly
        /// </summary>
        /// <remarks>
        /// Does special try/catch to prevent bugs when assemblies are missing
        /// Source: http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes 
        /// </remarks>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly, ILog log = null)
        {
            // try to log
            try
            {
                log?.Add($"GetLoadableTypes(assembly: {assembly.FullName})");
            }
            catch {  /*ignore */}

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                log?.Add($"had ReflectionTypeLoadException {e.Message} \n" +
                         "will continue with using Types.Where");
                return e.Types.Where(t => t != null);
            }
        }
    }
}
