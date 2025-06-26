namespace ToSic.Eav.Persistence.Efc.Sys.TempModels;

internal class TempAttributeWithValues
{
    public required string Name;
    public required ICollection<TempValueWithLanguage> Values;
}