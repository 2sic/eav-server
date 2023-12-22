using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Code to re-wrap stuff inside a stack, so it could navigate through stacks again if necessary
/// </summary>
internal class StackReWrapper: HelperBase
{
    public StackReWrapper(StackAddress stackAddress, ILog parentLog): base(parentLog, "Eav.StkWrp") => StackAddress = stackAddress;
    public StackAddress StackAddress { get; }

    public PropReqResult ReWrapIfPossible(PropReqResult reqResult) => Log.Func(l =>
    {
        var result = reqResult.Result;

        // Skip if not relevant
        if (result == null)
            return (reqResult, "null result");

        // Skip if already wrapped / has stack navigation
        // Note: ATM there is no known case where we just receive one item back (just one IEntity, ...)
        if (reqResult.ResultOriginal != null
            || result is IPropertyStackLookup
            || result is IEnumerable<IPropertyStackLookup>
           )
            return (reqResult, "already wrapped");


        // List of IEntities
        if (result is IEnumerable<IEntity> entityChildren)
        {
            l.A("Result is IEnumerable<IEntity> - will wrap in navigation");
            reqResult = WrapResultInOwnStack(reqResult, entityChildren,
                (ent, address) => new EntityWithStackNavigation(ent, address));
            return (reqResult, "wrapped as Entity-Stack, final");
        }


        // 2022-12-01 2dm - new type of result, mainly for testing ATM, shouldn't happen in production
        // It seems to be necessary as soon as the result is a field-property list
        // But the parent seems off...
        // I'm not using IEnumerable<IPropertyLookup> because that could have untested side-effects
        if (result is IEnumerable<PropertyLookupDictionary> dicChildren)
        {
            l.A("Result is IEnumerable<PropertyLookupDictionary> - will wrap in navigation");
            reqResult = WrapResultInOwnStack(reqResult, dicChildren,
                (obj, address) => new PropertyLookupWithStackNavigation(obj, address));
            return (reqResult, "wrapped as dictionary prop stack, final");
        }

        return (reqResult, "result not modified");
    });

    private PropReqResult WrapResultInOwnStack<TOriginal, TResult>(
        PropReqResult reqResult,
        IEnumerable<TOriginal> dicChildren,
        Func<TOriginal, StackAddress, TResult> factory
    ) where TOriginal : IPropertyLookup => Log.Func(() =>
    {
        reqResult.ResultOriginal = reqResult.Result;

        var children = dicChildren.ToArray();
        var newStackForTheChildren =
            new PropertyStack().Init($"ContentsOf-{StackAddress.Field}", children as IPropertyLookup[]);
        var childrenStackAddress = StackAddress.Child(newStackForTheChildren, newStackForTheChildren.NameId, 0);

        reqResult.Result = children
            .Select((c, i) => factory(c, childrenStackAddress.NewWithOtherIndex(i)))
            .ToList();

        return (reqResult);
    });
}