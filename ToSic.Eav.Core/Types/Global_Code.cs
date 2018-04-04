using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Types.Attributes;

namespace ToSic.Eav.Types
{
    public partial class Global
    {




        /// <summary>
        /// Initialized CodeContentTypes
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, IContentType> CodeContentTypes()
        {
            if (_codeContentTypes != null) return _codeContentTypes;

            _codeContentTypes = new Dictionary<string, IContentType>(StringComparer.OrdinalIgnoreCase);

            // 2018-03-09 2dm disabled all this, because ATM we don't have coded type any more
            // keeping it active caused some difficult recursions and stack-overflows, as the lists were always empty
            //_codeContentTypes = ContentTypesInReflection()
            //    .Select(type => new
            //    {
            //        Name = (type.GetCustomAttributes(typeof(ContentTypeDefinition), true)
            //            .FirstOrDefault() as ContentTypeDefinition)?.StaticName,
            //        Obj = (IContentType) Activator.CreateInstance(type)
            //    })
            //    .ToDictionary(t => t.Name, t => t.Obj, StringComparer.OrdinalIgnoreCase); // make sure it's case insensitive...
            return _codeContentTypes;
        }

        private static Dictionary<string, IContentType> _codeContentTypes;





        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Type> ContentTypesInReflection()
            => _typeCache ?? (_typeCache =
                   //Plumbing.AssemblyHandling.FindInherited(typeof(ContentType));
                   Plumbing.AssemblyHandling.FindClassesWithAttribute(typeof(IContentType),
                       typeof(ContentTypeDefinition), true));
        private static IEnumerable<Type> _typeCache;


        //public static ContentType InstanceOf(string name)
        //{
        //    var type = ContentTypesInReflection()
        //        .FirstOrDefault(t =>
        //            (t.GetCustomAttributes(typeof(ContentTypeDefinition), true).FirstOrDefault() as ContentTypeDefinition)
        //            ?.StaticName == name);

        //    return type == null ? null : (ContentType) Activator.CreateInstance(type);
        //}
    }
}
