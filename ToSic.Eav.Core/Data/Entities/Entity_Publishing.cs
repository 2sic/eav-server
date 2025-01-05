namespace ToSic.Eav.Data;

partial record Entity
{
    /// <inheritdoc />
    [PrivateApi]
    public required int RepositoryId { get; init; }

    #region Save/Update settings - needed when passing this object to the save-layer

    /// <inheritdoc />
    // TODO: should move the set-info to a save-options object
    public required bool IsPublished { get; set; } = true;

    internal int? PublishedEntityId { get; init; }

    // TODO: should move the set-info to a save-options object
    [PrivateApi]
    public bool PlaceDraftInBranch { get; set; }

    #endregion


    /// <inheritdoc />
    public required int Version { get; init; }

}