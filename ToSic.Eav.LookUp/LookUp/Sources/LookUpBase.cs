namespace ToSic.Eav.LookUp.Sources;

/// <summary>
/// Base Class to create your own LookUp Class - used by all Look-Ups. <br/>
/// Read more about this in [](xref:Abyss.Parts.LookUp.Index)
/// </summary>
[PublicApi]
public abstract class LookUpBase(string name, string? description = "") : ILookUp
{

    #region default methods of interface
    /// <inheritdoc/>
    public string Name { get; } = name ?? throw new NullReferenceException("LookUp must have a Name");

    public virtual string Description => description ?? "";

    /// <inheritdoc/>
    public abstract string Get(string key, string format);

    /// <inheritdoc/>
    public virtual string Get(string key) => Get(key, "");

    #endregion


    public override string ToString() => $"{GetType().Name}; {Description}";

    ILookUp ICanBeLookUp.LookUp => this;
}