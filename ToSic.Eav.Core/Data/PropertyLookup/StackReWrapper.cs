using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.PropertyLookup
{
    /// <summary>
    /// Code to re-wrap stuff inside a stack, so it could navigate through stacks again if necessary
    /// </summary>
    internal class StackReWrapper: HasLog
    {

        public StackReWrapper(StackAddress stackAddress, ILog parentLog): base("Eav.StkWrp", parentLog) => StackAddress = stackAddress;
        public StackAddress StackAddress { get; }

        public PropReqResult ReWrapIfPossible(PropReqResult reqResult)
        {
            var l = Log.Fn<PropReqResult>();

            var result = reqResult.Result;

            // Skip if not relevant
            if (result == null) 
                return l.Return(reqResult, "null result");

            // Skip if already wrapped / has stack navigation
            // Note: ATM there is no known case where we just receive one item back (just one IEntity, ...)
            if (reqResult.ResultOriginal != null
                || result is IPropertyStackLookup
                || result is IEnumerable<IPropertyStackLookup>
               )
                return l.Return(reqResult, "already wrapped");


            // List of IEntities
            if (result is IEnumerable<IEntity> entityChildren)
            {
                l.A("Result is IEnumerable<IEntity> - will wrap in navigation");
                reqResult = WrapResultInOwnStack(reqResult, entityChildren, (ent, address) => new EntityWithStackNavigation(ent, address));
                return l.Return(reqResult, "wrapped as Entity-Stack, final");
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
                return l.Return(reqResult, "wrapped as dictionary prop stack, final");
            }

            return l.Return(reqResult, "result not modified");
        }

        private PropReqResult WrapResultInOwnStack<TOriginal, TResult>(
            PropReqResult reqResult, 
            IEnumerable<TOriginal> dicChildren,
            Func<TOriginal, StackAddress, TResult> factory) where TOriginal: IPropertyLookup
        {
            var l = Log.Fn<PropReqResult>();
            reqResult.ResultOriginal = reqResult.Result;

            var children = dicChildren.ToArray();
            var newStackForTheChildren = new PropertyStack().Init($"ContentsOf-{StackAddress.Field}", children as IPropertyLookup[]);
            var childrenStackAddress = StackAddress.Child(newStackForTheChildren, newStackForTheChildren.NameId, 0);

            reqResult.Result = children
                .Select((c, i) => factory(c, childrenStackAddress.NewWithOtherIndex(i)))
                .ToList();

            return l.Return(reqResult);
        }
    }
}
