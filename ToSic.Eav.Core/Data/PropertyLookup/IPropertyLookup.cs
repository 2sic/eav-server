using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data.PropertyLookup
{
    [PrivateApi]
    public interface IPropertyLookup
    {
        /// <summary>
        /// Internal helper to get a property with additional information for upstream processing. 
        /// </summary>
        /// <returns></returns>
        PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path);

        [PrivateApi]
        List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path);

    }
}
