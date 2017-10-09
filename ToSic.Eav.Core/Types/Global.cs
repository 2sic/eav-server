using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types.Attributes;

namespace ToSic.Eav.Types
{
    public class Global
    {
        private static IEnumerable<Type> _typeCache;

        internal static IEnumerable<Type> ContentTypesInReflection()
            => _typeCache ?? (_typeCache =
                   //Plumbing.AssemblyHandling.FindInherited(typeof(ContentType));
                   Plumbing.AssemblyHandling.FindClassesWithAttribute(typeof(IContentType),
                       typeof(ContentTypeDefinition), true));

        private static Dictionary<string, IContentType> _contentTypeCache;

        public static Dictionary<string, IContentType> SystemContentTypes()
        {
            if (_contentTypeCache != null) return _contentTypeCache;
            _contentTypeCache = ContentTypesInReflection()
                .Select(type => new
                {
                    Name = (type.GetCustomAttributes(typeof(ContentTypeDefinition), true)
                        .FirstOrDefault() as ContentTypeDefinition)?.StaticName,
                    Obj = (IContentType) Activator.CreateInstance(type)
                })
                .ToDictionary(t => t.Name, t => t.Obj);
            _contentTypeCache = new Dictionary<string, IContentType>(_contentTypeCache, StringComparer.OrdinalIgnoreCase);
            return _contentTypeCache;
        }

        public static IContentType SystemContentType(string name)
        {
            var types = SystemContentTypes();
            return types.ContainsKey(name) ? types[name] : null;
        }

        public static ContentType InstanceOf(string name)
        {
            var type = ContentTypesInReflection()
                .FirstOrDefault(t =>
                    (t.GetCustomAttributes(typeof(ContentTypeDefinition), true).FirstOrDefault() as ContentTypeDefinition)
                    ?.StaticName == name);

            return type == null ? null : (ContentType) Activator.CreateInstance(type);
        }
    }
}
