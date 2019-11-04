using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	[PublicApi]
	public interface IValueCollectionProvider
	{
		/// <summary>
		/// Property Sources this Provider can use. Sources are various dictionaries which can resolve a key to a value. 
		/// </summary>
		/// <returns><see cref="Dictionary{TKey,TValue}"/> of <see cref="IValueProvider"/> items.</returns>
		Dictionary<string, IValueProvider> Sources { get; }

	    /// <summary>
	    /// Replaces all Tokens in the ConfigList with actual values provided by the Sources-Provider
	    /// </summary>
	    void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, IValueProvider> instanceSpecificPropertyAccesses = null, int repeat = 2);

	    /// <summary>
	    /// Add (or replace) a value provider in the source list
	    /// </summary>
	    /// <param name="newSource">An source to add to this configuration provider. The name will be taken from this object.</param>
        void Add(IValueProvider newSource);

        /// <summary>
        /// Add an overriding source. <br/>
        /// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
        /// </summary>
        /// <param name="propertyProvider">a <see cref="IValueProvider"/> which should override the original configuration</param>
        void AddOverride(IValueProvider propertyProvider);

        /// <summary>
        /// Add many overriding sources. <br/>
        /// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
        /// </summary>
        /// <param name="providers">list of <see cref="IValueProvider"/> which should override the original configuration</param>
	    void AddOverride(IEnumerable<IValueProvider> providers);

	}
}
