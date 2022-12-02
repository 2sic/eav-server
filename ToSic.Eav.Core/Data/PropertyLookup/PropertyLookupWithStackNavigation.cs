using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Test code!
    /// </summary>
    [PrivateApi]
    public class PropertyLookupWithStackNavigation : Wrapper<PropertyLookupDictionary>, IPropertyLookup, IHasIdentityNameId
    {
        public PropertyLookupWithStackNavigation(PropertyLookupDictionary current, IPropertyStackLookup parent, string field, int index, int depth) : base(current)
        {
            PropertyStackNavigator = new PropertyStackNavigator(current, parent, field, index, depth);
        }

        internal readonly PropertyStackNavigator PropertyStackNavigator;

        public PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".EntNav");
            var wrapLog = logOrNull.Fn<PropertyRequest>($"Source: {(_contents as IHasIdentityNameId)?.NameId}, {nameof(field)}: {field}");
            var result = PropertyStackNavigator.PropertyInStack(field, languages, 0, true, logOrNull, path);

            return wrapLog.Return(result, result?.Result != null ? "found" : null);
        }

        public List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull) 
            => _contents._Dump(languages, path, parentLogOrNull);

        public string NameId => (_contents as IHasIdentityNameId)?.NameId;
    }
}
