using System.Collections.Generic;
using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data
{
    public partial class PropertyStack
    {

        public IPropertyStack GetStack(params string[] names) => GetStack(null, names);

        public IPropertyStack GetStack(ILog log, params string[] names) => log.Func(l =>
        {
            // Get all required names in the order they were requested
            var newSources = new List<KeyValuePair<string, IPropertyLookup>>();
            foreach (var name in names)
            {
                var s = GetSource(name);
                l.A($"Add stack {name}, found: {s != null}");
                if (s != null) newSources.Add(new KeyValuePair<string, IPropertyLookup>(name, s));
            }

            var newStack = new PropertyStack();
            newStack.Init("New", newSources.ToArray());
            return (newStack, newSources.Count.ToString());
        });
    }
}
