using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ToSic.Lib.Logging;

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
            log.A($"FindInherited of type {type.FullName}");
            return GetTypes(log).Where(t => type.IsAssignableFrom(t) && (!t.IsAbstract || t.IsInterface) && t != type);
        }

        /// <summary>
        /// Check if specific type exists (eg. assembly is loaded from bin)
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static bool HasType(string typeFullName, ILog log = null)
        {
            var wrapLog = log.Fn<bool>(message: $"HasType {typeFullName}");
            return wrapLog.ReturnAsOk(GetTypes(log).Any(t => (t.FullName?.IndexOf(typeFullName, StringComparison.OrdinalIgnoreCase) ?? -1) > -1));
        }

        /// <summary>
        /// Return Type if it exists, or null if not
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static Type GetTypeOrNull(string typeFullName, ILog log = null)
        {
            var wrapLog = log.Fn<Type>(message: $"HasType {typeFullName}");
            return wrapLog.ReturnAsOk(GetTypes(log).FirstOrDefault(t => (t.FullName?.IndexOf(typeFullName, StringComparison.OrdinalIgnoreCase) ?? -1) > -1));
        }

        internal static List<Type> GetTypes(ILog log = null)
        {
            if (_typeCache != null) return _typeCache;

            var wrapLog = log.Fn<List<Type>>(timer: true);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            wrapLog.A($"GetTypes() - found {assemblies.Length} assemblies");

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
                log.A($"GetLoadableTypes(assembly: {assembly.FullName})");
            }
            catch {  /*ignore */}

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                log.A($"had ReflectionTypeLoadException {e.Message} \n" +
                         "will continue with using Types.Where");
                return e.Types.Where(t => t != null);
            }
            catch (Exception ex)
            {
                log.A($"None of the types from assembly '{assembly?.FullName}' could be loaded.");
                log.Ex(ex);
                return Enumerable.Empty<Type>();
            }
        }
    }
}
