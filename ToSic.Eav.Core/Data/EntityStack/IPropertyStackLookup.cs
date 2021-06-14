using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Should be an entity-reader which has a stack of entities it tries to access and prioritize which ones are to be asked first.
    /// </summary>
    [PrivateApi("internal only - don't publish in docs, can change at any time")]
    public interface IPropertyStackLookup
    {
        PropertyRequest PropertyInStack(string fieldName, string[] dimensions, int startAtSource, bool treatEmptyAsDefault);

    }
}
