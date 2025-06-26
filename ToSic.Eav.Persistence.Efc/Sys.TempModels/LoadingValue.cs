namespace ToSic.Eav.Persistence.Efc.Sys.TempModels;

internal record LoadingValue(
    int EntityId,
    int AttributeId,
    string StaticName,
    string Value,
    ICollection<ILanguage> Languages
);
