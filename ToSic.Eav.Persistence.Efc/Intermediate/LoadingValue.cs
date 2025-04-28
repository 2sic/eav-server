namespace ToSic.Eav.Persistence.Efc.Intermediate;

internal record LoadingValue(
    int EntityId,
    int AttributeId,
    string StaticName,
    string Value,
    int ChangeLogCreated,
    ICollection<ToSicEavValuesDimensions> ToSicEavValuesDimensions
    //List<ILanguage> Dimensions
);

internal record LoadingLanguage(string EnvKey, bool ReadOnly, int DimensionId);