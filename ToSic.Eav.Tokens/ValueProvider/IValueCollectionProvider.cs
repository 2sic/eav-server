using System.Collections.Generic;

namespace ToSic.Eav.ValueProvider
{
	/// <summary>
	/// Provides Configuration for a configurable DataSource
	/// </summary>
	public interface IValueCollectionProvider
	{
		//string DataSourceType { get; }
		/// <summary>
		/// Property Sources this Provider can use
		/// </summary>
		Dictionary<string, IValueProvider> Sources { get; }

	    /// <summary>
	    /// Replaces all Tokens in the ConfigList with actual values provided by the Sources-Provider
	    /// </summary>
	    void LoadConfiguration(IDictionary<string, string> configList, Dictionary<string, IValueProvider> instanceSpecificPropertyAccesses = null, int repeat = 2);
	}
}
