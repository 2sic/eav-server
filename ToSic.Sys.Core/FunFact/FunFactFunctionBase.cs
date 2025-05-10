using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Services;

namespace ToSic.Lib.FunFact;

/// <summary>
/// WIP fluid functional object factory.
/// The idea is to have a fluent API that allows you to create objects and apply actions to them in a functional way.
///
/// So every method will return a new object, basically queueing up what should be done to the object once created.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="parentLog"></param>
/// <param name="functions"></param>
/// <param name="logName"></param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class FunFactFunctionBase<T>(ILog? parentLog, IEnumerable<(string, Func<T, T>)>? functions, string? logName)
    : HelperBase(parentLog, logName ?? "Eav.FnOExp")
{
    /// <summary>
    /// List of actions to apply to the object
    /// </summary>
    protected internal List<(string Info, Func<T, T> Action)> Actions { get; } = functions?.ToList() ?? [];

    protected List<(string, Func<T, T>)> CloneActions(Func<T, T> addition)
        => [..functions ?? [], ("", addition)];

    protected List<(string, Func<T, T>)> CloneActions((string, Func<T, T>) addition)
        => [..functions ?? [], addition];

    protected T Apply(T initial)
    {
        var l = Log.Fn<T>($"{Actions.Count} action");
        var result = Actions.Aggregate(initial, (current, action) =>
        {
            l.A(action.Info);
            return action.Action(current);
        });
        return l.Return(result!);
    }

    public abstract T CreateResult();
}