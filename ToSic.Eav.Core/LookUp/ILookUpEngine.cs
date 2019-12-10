using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Resolves Configurations from LookUps. Common use is for configurable DataSources
    /// </summary>
    [PublicApi]
    public interface ILookUpEngine
	{
        /// <summary>
        /// Property Sources this Provider can use.
        /// Sources are various dictionaries which can resolve a key to a value. <br/>
        /// Read more about this in @Specs.LookUp.Intro
        /// </summary>
        /// <returns><see cref="Dictionary{TKey,TValue}"/> of <see cref="ILookUp"/> items.</returns>
        Dictionary<string, ILookUp> Sources { get; }

	    /// <summary>
	    /// Replaces all Tokens in the ConfigList with actual values provided by the LookUps in the Sources
	    /// </summary>
	    void LookUp(IDictionary<string, string> configList, Dictionary<string, ILookUp> instanceSpecificPropertyAccesses = null, int repeat = 2);

	    /// <summary>
	    /// Add (or replace) a value provider in the source list
	    /// </summary>
	    /// <param name="lookUp">An source to add to this configuration provider. The name will be taken from this object.</param>
        void Add(ILookUp lookUp);

        /// <summary>
        /// Add an overriding source. <br/>
        /// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
        /// </summary>
        /// <param name="lookUp">a <see cref="ILookUp"/> which should override the original configuration</param>
        void AddOverride(ILookUp lookUp);

        /// <summary>
        /// Add many overriding sources. <br/>
        /// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
        /// </summary>
        /// <param name="lookUps">list of <see cref="ILookUp"/> which should override the original configuration</param>
	    void AddOverride(IEnumerable<ILookUp> lookUps);

	}
}
