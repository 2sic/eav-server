using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data;

partial class PropertyStack
{

    public IPropertyStack GetStack(params string[] names) => GetStack(null, names);

    public IPropertyStack GetStack(ILog log, params string[] names)
    {
        var l = log.Fn<IPropertyStack>();
        // Get all required names in the order they were requested
        var newSources = new List<KeyValuePair<string, IPropertyLookup>>();
        foreach (var name in names)
        {
            var s = GetSource(name);
            l.A($"Add stack {name}, found: {s != null}");
            if (s != null) newSources.Add(new(name, s));
        }

        var newStack = new PropertyStack();
        newStack.Init("New", newSources.ToArray());
        return l.Return(newStack, newSources.Count.ToString());
    }
}