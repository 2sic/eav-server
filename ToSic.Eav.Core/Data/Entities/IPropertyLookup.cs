using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public interface IPropertyLookup
    {
        /// <summary>
        /// Internal helper to get a property with additional information for upstream processing. 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="languages"></param>
        /// <returns></returns>
        [PrivateApi("WIP, internal 12.02")]
        PropertyRequest FindPropertyInternal(string fieldName, string[] languages);

    }
}
