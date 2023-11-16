﻿using System;
using System.Collections.Generic;
using ToSic.Eav.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("Internal stuff only")]
    public interface IFeaturesInternal: IFeaturesService, ICacheExpiring
    {
        [PrivateApi]
        IEnumerable<FeatureState> All { get; }

        [PrivateApi]
        IEnumerable<FeatureState> UiFeaturesForEditors { get; }


        [PrivateApi]
        bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception);

        [PrivateApi]
        string MsgMissingSome(params Guid[] ids);

        /// <summary>
        /// Checks if a list of features are enabled, in case you need many features to be activated.
        /// </summary>
        /// <param name="nameIds">list/array of name IDs</param>
        /// <returns>true if all features are enabled, false if any one of them is not</returns>
        /// <remarks>
        /// Added in v13.01
        /// </remarks>
        [PrivateApi("Hide - was never public on this interface")]
        bool IsEnabled(params string[] nameIds);

        FeatureState Get(string nameId);

        [PrivateApi("New in 13.05, not public at all")]
        bool IsEnabled(params FeatureDefinition[] features);


        FeatureListStored Stored { get; }
        event EventHandler FeaturesChanged;

        bool UpdateFeatureList(FeatureListStored newList, List<FeatureState> sysFeatures);
    }
}
