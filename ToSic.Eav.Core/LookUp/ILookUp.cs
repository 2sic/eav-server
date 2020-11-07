using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// A key-value map which resolves keys like "SortOrder" to "asc". <br/>
    /// It's usually used to get pre-stored configuration or to get settings from the context. <br/>
    /// Read more about this in @Specs.LookUp.Intro
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	public interface ILookUp
	{
		/// <summary>
		/// Gets the Name of this LookUp, e.g. QueryString or PipelineSettings
		/// </summary>
		/// <returns>The name which is used to identify this LookUp, like in a <see cref="ILookUpEngine"/></returns>
		string Name { get; }

        /// <summary>
        /// Gets a value by Name/key, will simply return the string or an empty string, in rare cases a null-value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The resolved value, or an empty string if not found. Note that it could also resolve to an empty string if found - use Has to check for that case.</returns>
	    string Get(string key);

        /// <summary>
        /// Gets a value by Name/key and tries to format it in a special way (like for dates)
        /// </summary>
        /// <param name="key">Name of the Property</param>
        /// <param name="format">Format String</param>
        ///// <returns>A string with the result, empty-string if not found.</returns>
        string Get(string key, string format);
    }
}