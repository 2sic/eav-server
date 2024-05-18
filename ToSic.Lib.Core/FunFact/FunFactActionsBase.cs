using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
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
/// <param name="actions"></param>
/// <param name="logName"></param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class FunFactActionsBase<T>(ILog parentLog, IEnumerable<Action<T>> actions, string logName) : HelperBase(parentLog, logName ?? "Eav.FnOExp")
{
    /// <summary>
    /// List of actions to apply to the object
    /// </summary>
    protected List<Action<T>> Actions { get; } = actions?.ToList() ?? [];

    protected List<Action<T>> CloneActions(Action<T> addition) => [..actions, addition];

    protected T Apply(T initial)
    {
        foreach (var action in Actions)
            action(initial);
        return initial;
    }

    public abstract T CreateResult();
}