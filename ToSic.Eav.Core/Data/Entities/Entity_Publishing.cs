namespace ToSic.Eav.Data;

partial class Entity
{
    /// <inheritdoc />
    [PrivateApi]
    public int RepositoryId { get; }

    #region Save/Update settings - needed when passing this object to the save-layer

    /// <inheritdoc />
    // TODO: should move the set-info to a save-options object
    public bool IsPublished { get; set; } = true;

    internal int? PublishedEntityId { get; }

    // TODO: should move the set-info to a save-options object
    [PrivateApi]
    public bool PlaceDraftInBranch { get; set; }

    #endregion


    /// <inheritdoc />
    public int Version { get; }

}