namespace ToSic.Eav.Persistence.Efc.Sys.TempModels;

internal class TempAttributeWithValues
{
    //public int AttributeId;
    public string Name;
    public ICollection<TempValueWithLanguage> Values;
}