namespace ToSic.Eav.Implementations.ValueConverter
{
    public interface IEavValueConverter
    {
        string Convert(ConversionScenario scenario, string type, string originalValue);

    }
}
