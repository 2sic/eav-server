using System.Collections.Generic;

namespace ToSic.Lib.FunFact;

/// <summary>
/// WIP fluid functional object factory.
/// With strings, it probably doesn't offer any benefits, so this is mostly for demonstration / unit-testing purposes.
/// </summary>
/// <param name="parentLog"></param>
/// <param name="actions"></param>
internal class FunFactString(ILog? parentLog, IEnumerable<(string, Func<string, string>)> actions) : FunFactFunctionBase<string>(parentLog, actions, "Lib.FunStr")
{
    public override string CreateResult() => Apply("");

    private FunFactString Next(string info, Func<string, string> addition)
        => new((Log as Log)?.Parent, CloneActions((info, addition)));

    public FunFactString Set(string value)
        => Next($"set:{value}", _ => value);

    public FunFactString Append(string text)
        => Next($"append:{text}", s => s + text);

    public FunFactString Prepend(string text)
        => Next($"prepend:{text}", s => text + s);

    public FunFactString Trim()
        => Next("trim", s => s.Trim());

    public FunFactString Replace(string oldValue, string newValue)
        => Next($"replace:'{oldValue}' with '{newValue}'", s => s.Replace(oldValue, newValue));
}