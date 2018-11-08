using System.Collections.Generic;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public interface IValueCollectionProvider
	{
		/// <summary>
		/// Property Sources this Provider can use
		/// </summary>
		Dictionary<string, IValueProvider> Sources { get; }

	    /// <summary>
	    /// Replaces all Tokens in the ConfigList with actual values provided by the Sources-Provider
	    /// </summary>
	    void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, IValueProvider> instanceSpecificPropertyAccesses = null, int repeat = 2);

	    /// <summary>
	    /// Add (or replace) a value provider in the source list
	    /// </summary>
	    /// <param name="newSource"></param>
        void Add(IValueProvider newSource);

        /// <summary>
        /// Add an overriding source
        /// </summary>
        /// <param name="propertyProvider"></param>
        void AddOverride(IValueProvider propertyProvider);

        /// <summary>
        /// Add many overriding sources
        /// </summary>
        /// <param name="providers"></param>
	    void AddOverride(IEnumerable<IValueProvider> providers);

	}
}
