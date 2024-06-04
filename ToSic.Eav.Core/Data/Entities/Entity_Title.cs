namespace ToSic.Eav.Data;

partial class Entity
{
    /// <inheritdoc />
    public new IAttribute Title => TitleFieldName == null
        ? null
        : Attributes?.ContainsKey(TitleFieldName) ?? false ? Attributes[TitleFieldName] : null;


    /// <inheritdoc />
    public new string GetBestTitle() => GetBestTitle(null, 0);

    /// <inheritdoc />
    public string GetBestTitle(string[] dimensions) => GetBestTitle(dimensions, 0);

    /// <inheritdoc />
    internal string GetBestTitle(string[] dimensions, int recursionCount)
    {
        // wrap it try-catch, because in rare cases the title may not be set or something else could break
        try
        {
            var bestTitle = Get(Data.Attributes.EntityFieldTitle, languages: dimensions);

            // in case the title is an entity-picker and has items, try to ask it for the title
            // note that we're counting recursions, just to be sure it won't loop forever
            if (bestTitle is IEnumerable<IEntity> titleRelationship && titleRelationship.Any())
                if (recursionCount < 3) // && (maybeRelationship?.Any() ?? false))
                    bestTitle = (titleRelationship.FirstOrDefault() as Entity)?
                                .GetBestTitle(dimensions, recursionCount + 1)
                                ?? bestTitle;

            return bestTitle?.ToString();
        }
        catch
        {
            return null;
        }
    }
}