using System;
using ToSic.Eav.Data;

namespace ToSic.Testing.Shared.Mocks
{
    /// <summary>
    /// Will pretend to convert links beginning with "page:" or "link:"
    /// </summary>
    public class MockValueConverter:IValueConverter
    {

        public string ToReference(string value)
        {
            throw new NotSupportedException("Not supported in provider 'Mock'");
        }

        public string ToValue(string reference, Guid itemGuid)
        {
            var refLower = reference.ToLowerInvariant();
            return refLower.StartsWith(ValueConverterBase.PrefixPage + ValueConverterBase.Separator ) 
                   || refLower.StartsWith(ValueConverterBase.PrefixFile + ValueConverterBase.Separator)
                ? "http://mock.converted/" + reference
                : reference;
        }
    }
}
