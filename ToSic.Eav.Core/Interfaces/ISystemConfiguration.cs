﻿namespace ToSic.Eav.Interfaces
{
    public interface ISystemConfiguration
    {
        /// <summary>
        /// Db Connection String used in the Eav-Connector
        /// </summary>
        string DbConnectionString { get; }

        /// <summary>
        /// A help link to show the user when a feature isn't available and 
        /// he/she needs to know more
        /// </summary>
        string FeaturesHelpLink { get; }

        /// <summary>
        /// The root link as a prefix to the details-info-link for a feature
        /// </summary>
        string FeatureInfoLinkRoot { get; }
    }
}
