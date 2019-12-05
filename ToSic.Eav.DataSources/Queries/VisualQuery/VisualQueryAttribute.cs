using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// Custom Attribute for DataSources and usage in Pipeline Designer.
    /// Will add information about help, configuration-content-types etc.
    /// Only DataSources which have this attribute will be listed in the designer-tool
    /// </summary>
    [PublicApi]

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class VisualQueryAttribute : Attribute
	{
        /// <summary>
        /// Empty constructor - necessary, so DocFx includes this attribute in documentation.
        /// </summary>
        [PrivateApi]
        public VisualQueryAttribute()
        {

        }

	    /// <summary>
	    /// A primary type of this source, which determines a default icon + some standard help-text
	    /// </summary>
	    /// <returns>The type, from the <see cref="DataSourceType"/> enum</returns>
	    public DataSourceType Type { get; set; } = DataSourceType.Source;

        /// <summary>
        /// Optional custom icon, based on the icon-names from the FontAwesome 4 library.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// List of in-streams expected by this data-source - will be shown in the UI. Default is empty. 
        /// </summary>
        public string[] In { get; set; } = new string[0];

        /// <summary>
        /// Determine if this data sources can have many out-streams with custom names
        /// </summary>
        /// <returns>True if this data source can also provide other named out-streams, false if it only has the defined list of out-streams.</returns>
        public bool DynamicOut { get; set; } = false;

        /// <summary>
        /// The help-link to get help for this data source. The UI will offer a help-button if provided. 
        /// </summary>
	    public string HelpLink { get; set; } = "";

        /// <summary>
        /// Should configuration be enabled in the VisualQuery designer?
        /// </summary>
        /// <returns>True if we have a known configuration content-type</returns>
	    public bool EnableConfig => !string.IsNullOrWhiteSpace(ExpectsDataOfType);

        /// <summary>
        /// Name of the content-type used to configure this data-source in the visual-query designer. <br/>
        /// The UI will then open an edit-dialog for this content type. 
        /// _Should be a GUID. Older data sources have a name like "|Config ToSic.Eav.DataSources.App", but that's deprecated._
        /// </summary>
        public string ExpectsDataOfType { get; set; }


        /// <summary>
        /// Nice name - usually an override to an internal class-name which shouldn't change any more
        /// but is not correct from the current wording - like ContentTypeFilter instead of EntityTypeFilter. <br/>
        /// If not specified, the UI will use the normal name instead. 
        /// </summary>
        public string NiceName { get; set; }

        /// <summary>
        /// **required** this should be a unique id, ideally a GUID. <br/>
        /// _important: old code use string names like a.net namespace. This should not be done any more and will be deprecated in future._
               /// </summary>
	    public string GlobalName { get; set; } = "";

        /// <summary>
        /// Names this DataSource may have had previously. <br/>
        /// This was introduced when we standardized the names, and still had historic data using old names or old namespaces. 
        /// </summary>
	    public string[] PreviousNames { get; set; } = new string[0];

        /// <summary>
        /// This is still an experiment, but the goal is to hide complex sources from "normal" users
        /// </summary>
        [PrivateApi]
	    public DifficultyBeta Difficulty { get; set; } = DifficultyBeta.Default;

	}
}