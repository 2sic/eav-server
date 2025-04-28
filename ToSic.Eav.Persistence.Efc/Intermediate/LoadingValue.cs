namespace ToSic.Eav.Persistence.Efc.Intermediate;

internal record LoadingValue(
    int EntityId,
    int AttributeId,
    string StaticName,
    string Value,
    int ChangeLogCreated,
    ImmutableList<ILanguage> Languages
);
