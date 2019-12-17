using System;
using ToSic.Eav.Data;
using ToSic.Eav.Run;

namespace ToSic.Eav.Core.Tests.Mocks
{
    /// <summary>
    /// Will pretend to convert links beginning with "page:" or "link:"
    /// </summary>
    public class MockValueConverter:IValueConverter
    {

        public string ToReference(string value)
        {
            throw new NotImplementedException();
        }

        public string ToValue(string reference, Guid itemGuid)
        {
            return reference.ToLowerInvariant().StartsWith("page:") || reference.ToLowerInvariant().StartsWith("file:")
                ? "http://mock.converted/" + reference
                : reference;
        }
    }
}
