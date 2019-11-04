using ToSic.Eav.Documentation;

namespace ToSic.Eav.ValueProvider
{
    /// <summary>
    /// A key-value map which resolves keys like "SortOrder" to "asc". <br/>
    /// It's usually used to get pre-stored configuration or to get settings from the context.
    /// </summary>
    [PublicApi]
	public interface IValueProvider
	{
		/// <summary>
		/// Gets the Name of this ValueProvider, e.g. QueryString or PipelineSettings
		/// </summary>
		/// <returns>The name which is used in a <see cref="IValueCollectionProvider"/> to identify this source. </returns>
		string Name { get; }

		/// <summary>
		/// Gets a Property by Name and format it in a special way (like for dates)
		/// </summary>
		/// <returns>A string with the result, empty-string if not found.</returns>
		string Get(string property, string format, ref bool propertyNotFound);

        /// <summary>
        /// Shorthand version, will simply return the string or a null-value
        /// </summary>
        /// <param name="property"></param>
        /// <returns>The resolved value, or an empty string if not found. Note that it could also resolve to an empty string if found - use Has to check for that case.</returns>
	    string Get(string property);

        /// <summary>
        /// Checks if this value provider has a key.
        /// </summary>
        /// <param name="property">The key to check</param>
        /// <returns>true if found, false if not</returns>
	    bool Has(string property);
	}
}