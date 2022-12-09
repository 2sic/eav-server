﻿using System.Collections.Generic;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.PropertyLookup
{
    /// <summary>
    /// Test code!
    /// </summary>
    [PrivateApi]
    public class PropertyLookupWithStackNavigation : Wrapper<PropertyLookupDictionary>, IPropertyLookup, IPropertyStackLookup
    {
        public PropertyLookupWithStackNavigation(PropertyLookupDictionary current, StackAddress stackAddress) : base(current) 
            => Navigator = new PropertyStackNavigator(current, stackAddress);
        internal readonly PropertyStackNavigator Navigator;


        public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
            => FindPropertyInternalOfStackWrapper(this, specs, path, LogNames.Eav + ".PrpNav", $"Source: {_contents.NameId}");

        public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path) 
            => Navigator.GetNextInStack(specs, startAtSource, path);

        public List<PropertyDumpItem> _Dump(PropReqSpecs specs, string path) => _contents._Dump(specs, path);



        /// <summary>
        /// Shared method for other stack wrappers - will log and call the code
        /// </summary>
        public static PropReqResult FindPropertyInternalOfStackWrapper<T>(T parent, PropReqSpecs specs, PropertyLookupPath path, string logName, string logMessage) where T: IPropertyStackLookup
        {
            specs = specs.SubLog(logName);
            var l = specs.LogOrNull.Fn<PropReqResult>($"{logMessage}, {specs.Dump()}");
            var result = parent.GetNextInStack(specs, 0, path);
            return l.Return(result, result?.Result != null ? "found" : null);
        }
    }
}