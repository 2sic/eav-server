using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("Internal stuff only")]
    public interface IFeaturesInternal: IFeaturesService
    {
        [PrivateApi]
        IEnumerable<FeatureState> All { get; }

        [PrivateApi]
        IEnumerable<FeatureState> EnabledUi { get; }


        [PrivateApi]
        bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception);

        [PrivateApi]
        string MsgMissingSome(IEnumerable<Guid> ids);

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

        [PrivateApi("New in 13.05, not public at all")]
        bool IsEnabled(params FeatureDefinition[] features);


        ///// <summary>
        ///// A help link to show the user when a feature isn't available and 
        ///// he/she needs to know more
        ///// </summary>
        //string HelpLink { get; }

        ///// <summary>
        ///// The root link as a prefix to the details-info-link for a feature
        ///// </summary>
        //string InfoLinkRoot { get; }

        FeatureListStored Stored { get; set; }
        long CacheTimestamp { get; set; }
    }
}
