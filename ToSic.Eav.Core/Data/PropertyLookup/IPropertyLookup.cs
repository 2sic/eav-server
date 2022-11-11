using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public interface IPropertyLookup
    {
        /// <summary>
        /// Internal helper to get a property with additional information for upstream processing. 
        /// </summary>
        /// <returns></returns>
        [PrivateApi("WIP, internal 12.02")]
        PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull, PropertyLookupPath path);

        [PrivateApi]
        List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull);

    }
}
