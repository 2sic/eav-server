﻿using System;

namespace ToSic.Eav.DataSources.Attributes
{
	/// <inheritdoc />
	/// <summary>
	/// Custom Attribute for DataSources and usage in Pipeline Designer
	/// Only DataSources which have this attribute will be listed in the designer-tool
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class DataSourceProperties : Attribute
	{
        /// <summary>
        /// A primary type of this source, which determines a default icon + some standard help-text
        /// </summary>
	    public DataSourceType Type { get; set; }

        /// <summary>
        /// Optional customim icon
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// List of in-streams expected by this data-source
        /// </summary>
        public string[] In { get; set; } = new string[0];

        /// <summary>
        /// True if this data sources can have many out-streams with custom names
        /// </summary>
        public bool DynamicOut { get; set; } = false;

        /// <summary>
        /// The help-link to get help for this data source
        /// </summary>
	    public string HelpLink { get; set; } = "";


	    public bool EnableConfig { get; set; } = true;

        /// <summary>
        /// Name of the data-type it will support...
        /// </summary>
        public string ExpectsDataOfType { get; set; }

    }
}