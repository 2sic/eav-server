namespace ToSic.Eav.Data;

partial record Entity
{
    /// <inheritdoc />
    [PrivateApi]
    public required int RepositoryId { get; init; }

    #region Save/Update settings - needed when passing this object to the save-layer

    /// <inheritdoc />
    public required bool IsPublished { get; init; } = true;

    internal int? PublishedEntityId { get; init; }

    #endregion


    /// <inheritdoc />
    public required int Version { get; init; }

}