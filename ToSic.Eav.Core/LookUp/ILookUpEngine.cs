﻿namespace ToSic.Eav.LookUp;

/// <summary>
/// Resolves Configurations from LookUps. Common use is for configurable DataSources
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface ILookUpEngine: IHasLog
{
    /// <summary>
    /// Property Sources this Provider can use.
    /// Sources are various dictionaries which can resolve a key to a value. <br/>
    /// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public IEnumerable<ILookUp> Sources { get; }

    /// <summary>
    /// This will go through a dictionary of strings (usually configuration values) and replace all tokens in that string
    /// with whatever the token-resolver delivers. It's usually needed to initialize a DataSource. 
    /// </summary>
    /// <param name="values">Dictionary of configuration strings.</param>
    /// <param name="overrides">Optional override LookUps which would be consulted first.</param>
    /// <param name="depth">Max recursion when looking up tokens which return other tokens.</param>
    IDictionary<string, string> LookUp(IDictionary<string, string> values, IEnumerable<ILookUp> overrides = null, int depth = LookUpEngine.DefaultLookUpDepth);

    // 2024-05-06 2dm going functional, so add must happen at setup
    ///// <summary>
    ///// Add (or replace) a value provider in the source list
    ///// </summary>
    ///// <param name="lookUp">An source to add to this configuration provider. The name will be taken from this object.</param>
    //void Add(ILookUp lookUp);

    // 2024-05-06 2dm - no need for a public API of add-one
    ///// <summary>
    ///// Add an overriding source. <br/>
    ///// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
    ///// </summary>
    ///// <param name="lookUp">a <see cref="ILookUp"/> which should override the original configuration</param>
    //void AddOverride(ILookUp lookUp);

    ///// <summary>
    ///// Add many overriding sources. <br/>
    ///// This is used when the underlying configuration provider is shared, and this instance needs a few custom configurations. 
    ///// </summary>
    ///// <param name="lookUps">list of <see cref="ILookUp"/> which should override the original configuration</param>
    //void AddOverride(IEnumerable<ILookUp> lookUps);


    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    ILookUpEngine Downstream { get; }

    /// <summary>
    /// Find a source by name in the current engine or any downstream engines.
    /// </summary>
    /// <param name="name">the name we're looking for, invariant</param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    ILookUp FindSource(string name);

    /// <summary>
    /// Will check if the lookup engine - or any of it's downstream engines - have a source with the given name
    /// </summary>
    /// <param name="name">the name we're looking for, invariant</param>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    bool HasSource(string name);
}