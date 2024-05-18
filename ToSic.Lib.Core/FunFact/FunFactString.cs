using System;
using System.Collections.Generic;
using ToSic.Lib.Logging;

namespace ToSic.Lib.FunFact;

/// <summary>
/// WIP fluid functional object factory.
/// With strings, it probably doesn't offer any benefits, so this is mostly for demonstration / unit-testing purposes.
/// </summary>
/// <param name="parentLog"></param>
/// <param name="actions"></param>
internal class FunFactString(ILog parentLog, IEnumerable<Func<string, string>> actions) : FunFactFunctionBase<string>(parentLog, actions, "Lib.FunStr")
{
    public override string CreateResult() => Apply("");

    private FunFactString Next(Func<string, string> addition) => new((Log as Log)?.Parent, CloneActions(addition));

    public FunFactString Set(string value) => Next(s => value);

    public FunFactString Append(string text) => Next(s => s + text);

    public FunFactString Prepend(string text) => Next(s => text + s);

    public FunFactString Trim() => Next(s => s.Trim());

    public FunFactString Replace(string oldValue, string newValue) => Next(s => s.Replace(oldValue, newValue));
}