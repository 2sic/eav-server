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
/// <param name="functions"></param>
/// <param name="logName"></param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class FunFactFunctionBase<T>(ILog parentLog, IEnumerable<Func<T, T>> functions, string logName) : HelperBase(parentLog, logName ?? "Eav.FnOExp")
{
    /// <summary>
    /// List of actions to apply to the object
    /// </summary>
    protected internal List<Func<T, T>> Actions { get; } = functions?.ToList() ?? [];

    protected List<Func<T, T>> CloneActions(Func<T, T> addition) => [..functions, addition];

    protected T Apply(T initial) => Actions.Aggregate(initial, (current, action) => action(current));

    public abstract T CreateResult();
}