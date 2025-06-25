namespace ToSic.Eav.Data.Sys.Entities;

partial record Entity
{
    /// <inheritdoc />
    public IAttribute? Title => TitleFieldName == null
        ? null
        : this[TitleFieldName];


    /// <inheritdoc />
    public string? GetBestTitle() => GetBestTitle(null, 0);

    /// <inheritdoc />
    public string? GetBestTitle(string?[] dimensions) => GetBestTitle(dimensions, 0);

    internal string? GetBestTitle(string?[]? dimensions, int recursionCount)
    {
        // wrap it try-catch, because in rare cases the title may not be set or something else could break
        try
        {
            var bestTitle = Get(AttributeNames.EntityFieldTitle, languages: dimensions);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            if (bestTitle is not IEnumerable<IEntity> titleRelationship)
                return bestTitle?.ToString();

            var relsList = titleRelationship.ToListOpt();
            if (relsList.Any() && recursionCount < 3)
                bestTitle = (relsList.FirstOrDefault() as Entity)
                            ?.GetBestTitle(dimensions, recursionCount + 1)
                            ?? bestTitle;

            return bestTitle.ToString();
        }
        catch
        {
            return null;
        }
    }
}