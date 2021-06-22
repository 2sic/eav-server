using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    public interface IPropertyLookup
    {
        /// <summary>
        /// Internal helper to get a property with additional information for upstream processing. 
        /// </summary>
        /// <returns></returns>
        [PrivateApi("WIP, internal 12.02")]
        PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull);

    }
}
