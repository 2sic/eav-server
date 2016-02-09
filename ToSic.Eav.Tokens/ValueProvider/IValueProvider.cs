namespace ToSic.Eav.ValueProvider
{
	public interface IValueProvider
	{
		/// <summary>
		/// Gets the Name of the Property Accessor, e.g. QueryString or PipelineSettings
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Gets a Property by Name
		/// </summary>
		string Get(string property, string format, ref bool propertyNotFound);

        /// <summary>
        /// Shorthand version, will simply return the string or a null-value
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
	    string Get(string property);

	    bool Has(string property);
	}
}