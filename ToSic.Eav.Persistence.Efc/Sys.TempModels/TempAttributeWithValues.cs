namespace ToSic.Eav.Persistence.Efc.Sys.TempModels;

internal class TempAttributeWithValues
{
    //public int AttributeId;
    public string Name;
    public IEnumerable<TempValueWithLanguage> Values;
}