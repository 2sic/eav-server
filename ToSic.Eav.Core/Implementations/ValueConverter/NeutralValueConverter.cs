namespace ToSic.Eav.Implementations.ValueConverter
{
    public class NeutralValueConverter : IEavValueConverter
    {
        public string Convert(ConversionScenario scenario, string type, string originalValue) => originalValue;
    }
}