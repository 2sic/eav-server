using ToSic.Eav.Plumbing;
using ToSic.Sys.Utils;

namespace ToSic.Lib.GetByName;

public class ObjectWrapperCanGetByName(object? source): ICanGetByName
{
    private readonly IDictionary<string, object?>? _source = source == null
        ? null
        : source as IDictionary<string, object?>
          ?? (source.IsAnonymous()
              ? source.ObjectToDictionary(caseInsensitive: true)
              : null
          );

    public object? Get(string name) => _source == null
        ? null
        : _source.TryGetValue(name, out var value)
            ? value
            : null;
}