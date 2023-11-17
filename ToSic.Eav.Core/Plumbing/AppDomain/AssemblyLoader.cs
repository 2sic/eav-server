using System;
using System.IO;
using System.Reflection;

namespace ToSic.Eav.Plumbing
{
    public abstract class AssemblyLoader : MarshalByRefObject
    {
        public Type LoadAssemblyAndGetTypeInfo(string assemblyPath, string typeName)
        {
            try
            {
                // do not use Assembly.LoadFrom() as it will lock the file
                var assembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
                return assembly.GetType(typeName);
            }
            catch
            {
                // Handle exceptions appropriately
                return null;
            }
        }
    }
}