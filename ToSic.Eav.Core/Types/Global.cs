using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Types.Attributes;

namespace ToSic.Eav.Types
{
    public class Global
    {
        private static IEnumerable<Type> _typeCache;

        public static IEnumerable<Type> SystemTypes()
            => _typeCache ?? (_typeCache =
                   //Plumbing.AssemblyHandling.FindInherited(typeof(ContentType));
                   Plumbing.AssemblyHandling.FindClassesWithAttribute(typeof(ContentType),
                       typeof(ContentTypeDefinition), true));

        public static ContentType InstanceOf(string name)
        {
            var type = SystemTypes()
                .FirstOrDefault(t =>
                    (t.GetCustomAttributes(typeof(ContentTypeDefinition), true).First() as ContentTypeDefinition)
                    ?.StaticName == name);

            return type == null ? null : (ContentType) Activator.CreateInstance(type);
        }
    }
}
