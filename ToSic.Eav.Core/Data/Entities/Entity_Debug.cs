using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        
        [PrivateApi]
        public List<PropertyDumpItem> _Dump(string[] languages, string path, ILog parentLogOrNull)
        {
            if (!Attributes.Any()) return new List<PropertyDumpItem>();
            if (PropertyDumpItem.ShouldStop(path)) return new List<PropertyDumpItem>{PropertyDumpItem.DummyErrorShouldStop(path)};

            var pathRoot = string.IsNullOrEmpty(path) ? "" : path + PropertyDumpItem.Separator;

            // Check if we have dynamic children
            IEnumerable<PropertyDumpItem> resultDynChildren = null;
            var dynChildField = Type?.DynamicChildrenField;
            if (dynChildField != null)
                resultDynChildren = Children(dynChildField)
                    .Where(child => child != null)
                    .SelectMany(inner =>
                        inner._Dump(languages, pathRoot + inner.GetBestTitle(languages), parentLogOrNull));

            // Get all properties which are not dynamic children
            var childAttributes = Attributes
                .Where(att =>
                    att.Value.Type == DataTypes.Entity
                    && (dynChildField == null || !att.Key.Equals(dynChildField, StringComparison.InvariantCultureIgnoreCase)));
            var resultProperties = childAttributes
                .SelectMany(att
                    => Children(att.Key)
                        .Where(child => child != null) // unclear why this is necessary, but sometimes the entities inside seem to be non-existant on Resources
                        .SelectMany(inner
                            => inner._Dump(languages, pathRoot + att.Key, parentLogOrNull)))
                .ToList();

            // Get all normal properties
            var resultValues =
                Attributes
                    .Where(att => att.Value.Type != DataTypes.Entity && att.Value.Type != DataTypes.VoidEmpty)
                    .Select(att =>
                    {
                        var property = FindPropertyInternal(att.Key, languages, parentLogOrNull, new PropertyLookupPath().Add("EntityDump"));
                        var item = new PropertyDumpItem
                        {
                            Path = pathRoot + att.Key,
                            Property = property
                        };
                        return item;
                    })
                    .ToList();

            var finalResult = resultProperties.Concat(resultValues);
            if (resultDynChildren != null) finalResult = finalResult.Concat(resultDynChildren);

            return finalResult.OrderBy(f => f.Path).ToList();

        }
        
    }
}
