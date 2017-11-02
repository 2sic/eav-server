using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ToSic.Eav.Plumbing
{
    public class AssemblyHandling
    {

        /// <summary>
        /// Get all Installed DataSources
        /// </summary>
        /// <remarks>Objects that implement IDataSource</remarks>
        public static IEnumerable<Type> FindInherited(Type type)
            => Types.Where(t => type.IsAssignableFrom(t) && (!t.IsAbstract || t.IsInterface) && t != type);

        public static IEnumerable<Type> FindClassesWithAttribute(Type type, Type attribType, bool includeInherited)
            => FindInherited(type).Where(d => d.GetCustomAttributes(attribType, includeInherited).Any());

        private static IEnumerable<Type> Types
            => _typeCache ?? (_typeCache = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetLoadableTypes));
        private static IEnumerable<Type> _typeCache;

        /// <summary>
        /// Get Loadable Types from an assembly
        /// </summary>
        /// <remarks>
        /// Does special try/catch to prevent bugs when assemblies are missing
        /// Source: http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes 
        /// </remarks>
        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
