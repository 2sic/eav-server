using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Types.Attributes;

namespace ToSic.Eav.Types
{
    public class Global
    {
        public static IEnumerable<Type> SystemTypes()
        {
            var basicTypes =
                Plumbing.AssemblyHandling.FindInherited(typeof(ContentType));
                //Plumbing.AssemblyHandling.FindClassesWithAttribute(typeof(ContentType), 
                //typeof(ContentTypeDefinition), true);

            return basicTypes;
        }
    }
}
