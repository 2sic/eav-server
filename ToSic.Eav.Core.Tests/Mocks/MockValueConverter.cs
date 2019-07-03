using System;
using ToSic.Eav.Implementations.ValueConverter;

namespace ToSic.Eav.Core.Tests.Mocks
{
    /// <summary>
    /// Will pretend to convert links beginning with "page:" or "link:"
    /// </summary>
    public class MockValueConverter:IEavValueConverter
    {

        public string ToReference(string value)
        {
            throw new NotImplementedException();
        }

        public string ToValue(Guid itemGuid, string reference)
        {
            return reference.ToLowerInvariant().StartsWith("page:") || reference.ToLowerInvariant().StartsWith("file:")
                ? "http://mock.converted/" + reference
                : reference;
        }
    }
}
