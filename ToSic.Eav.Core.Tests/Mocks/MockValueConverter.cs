using ToSic.Eav.Implementations.ValueConverter;

namespace ToSic.Eav.Core.Tests.Mocks
{
    /// <summary>
    /// Will pretend to convert links beginning with "page:" or "link:"
    /// </summary>
    public class MockValueConverter:IEavValueConverter
    {
        public string Convert(ConversionScenario scenario, string type, string originalValue)
        {
            return originalValue.ToLowerInvariant().StartsWith("page:") || originalValue.ToLowerInvariant().StartsWith("file:")
                ? "http://mock.converted/" + originalValue
                : originalValue;
        }
    }
}
