﻿using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ToSic.Sys.FunctionalFactory;

/// <summary>
/// WIP fluid functional object factory.
/// The idea is to have a fluent API that allows you to create objects and apply actions to them in a functional way.
/// 
/// So every method will return a new object, basically queueing up what should be done to the object once created.
/// </summary>
/// <typeparam name="T"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record FunFactActionsBase<T> : HelperRecordBase
{
    /// <summary>
    /// List of actions to apply to the object
    /// </summary>
    [JsonIgnore] // Prevent System.Text.Json from serializing this property
    [IgnoreDataMember] // Prevent Newtonsoft Json from serializing this property, without depending on the Newtonsoft.Json package
    [PrivateApi]
    [field: AllowNull, MaybeNull]
    public List<(string Info, Action<T> Action)> Actions
    {
        get => field ??= [];
        init;
    }

    protected List<(string, Action<T>)> CloneActions((string, Action<T>) addition)
        => [..Actions, addition];

    protected T Apply(T initial)
    {
        var l = Log.Fn<T>($"{Actions.Count} actions");
        foreach (var action in Actions)
        {
            l.A(action.Info);
            action.Action(initial);
        }
        return l.Return(initial!);
    }

    public abstract T CreateResult();
}