using System;
using System.Linq;

namespace ToSic.Eav.Plumbing.Booting
{
    public static class Boot
    {
        public static void RunBootSequence()
        {
            var types = AssemblyHandling.FindInherited(typeof(IConfigurationLoader)).ToList();
            if (types.Count != 1)
                throw new Exception($"Tried to boot, but when looking for the {nameof(IConfigurationLoader)} I expected 1, but found {types.Count}");

            var config = (IConfigurationLoader)Activator.CreateInstance(types.First());

            config.Configure();
        }
    }
}
