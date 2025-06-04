namespace ToSic.Eav.Data.ContentTypes.Sys;

/// <summary>
/// todo
/// #SharedFieldDefinition
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentTypeAttributeSysSettings
{
    #region Sharing / Source

    /// <summary>
    /// Mark this Attribute that it shares itself / its properties
    /// </summary>
    public bool Share { get; init; }

    ///// <summary>
    ///// Tell the system that despite being shared, it won't be available for picking up in any UI.
    ///// This is for definitions that have sharing enabled to use once or twice, but then should not be available in UIs any more...?
    ///// </summary>
    //public bool ShareHidden { get; }

    #endregion

    /// <summary>
    /// Inherits-reference, ATM no purpose yet
    /// </summary>
    public Guid? Inherit { get; init; }

    /// <summary>
    /// Stored value - should usually NOT be used; ATM no purpose yet
    /// </summary>
    public bool InheritNameOfPrimary { get; init; }

    /// <summary>
    /// Stored value - should usually NOT be used, instead use InheritMetadata
    /// </summary>
    public bool InheritMetadataOfPrimary { get; init; }

    public Dictionary<Guid, string> InheritMetadataOf { get; init; }

    public bool InheritMetadata => InheritMetadataOf?.Any() == true || (Inherit != null && InheritMetadataOfPrimary);

    public Guid? InheritMetadataMainGuid => InheritMetadataOf?.Any() == true 
        ? InheritMetadataOf.FirstOrDefault().Key 
        : InheritMetadataOfPrimary
            ? Inherit
            : null;

}