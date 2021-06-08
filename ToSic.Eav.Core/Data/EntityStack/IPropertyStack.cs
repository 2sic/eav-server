using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Should be an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
    /// </summary>
    [PrivateApi("still WIP")]
    public interface IPropertyStack: IPropertyLookup
    {
        IPropertyLookup GetSource(string name);
        
        PropertyRequest FindPropertyInternal(string fieldName, string[] dimensions, bool treatEmptyAsDefault);

    }
}
