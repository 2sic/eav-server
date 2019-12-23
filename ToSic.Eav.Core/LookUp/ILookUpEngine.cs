using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// Resolves Configurations from LookUps. Common use is for configurable DataSources
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface ILookUpEngine: IHasLog
	{
        /// <summary>
        /// Property Sources this Provider can use.
        /// Sources are various dictionaries which can resolve a key to a value. <br/>
        /// Read more about this in @Specs.LookUp.Intro
        /// </summary>
        /// <returns><see cref="Dictionary{TKey,TValue}"/> of <see cref="ILookUp"/> items.</returns>
        Dictionary<string, ILookUp> Sources { get; }

        /// <summary>
        /// This will go through a dictionary of strings (usually configuration values) and replace all tokens in that string
        /// with whatever the token-resolver delivers. It's usually needed to initialize a DataSource. 
        /// </summary>
        /// <param name="values">Dictionary of configuration strings.</param>
        /// <param name="overrides">Optional override LookUps which would be consulted first.</param>
        /// <param name="depth">Max recursion when looking up tokens which return other tokens.</param>
        IDictionary<string, string> LookUp(IDictionary<string, string> values, IDictionary<string, ILookUp> overrides = null, int depth = LookUpEngine.DefaultLookUpDepth);

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
