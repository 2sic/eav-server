using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("Internal stuff only")]
    public interface IFeaturesInternal: IFeaturesService
    {
        [PrivateApi]
        IEnumerable<Feature> All { get; }

        [PrivateApi]
        IEnumerable<Feature> Ui { get; }


        [PrivateApi]
        bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception);

        [PrivateApi]
        string MsgMissingSome(IEnumerable<Guid> ids);



        /// <summary>
        /// A help link to show the user when a feature isn't available and 
        /// he/she needs to know more
        /// </summary>
        string HelpLink { get; }

        /// <summary>
        /// The root link as a prefix to the details-info-link for a feature
        /// </summary>
        string InfoLinkRoot { get; }

        FeatureList Stored { get; set; }
        long CacheTimestamp { get; set; }
    }
}
