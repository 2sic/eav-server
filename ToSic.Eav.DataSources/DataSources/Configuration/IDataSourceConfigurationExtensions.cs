namespace ToSic.Eav.DataSources
{
    public static class IDataSourceConfigurationExtensions
    {
        public static bool GetBool(this IDataSourceConfiguration config, string key) 
            => bool.TryParse(config[key], out var result) && result;

        public static void SetBool(this IDataSourceConfiguration config, string key, bool value) 
            => config[key] = value.ToString();
    }
}
