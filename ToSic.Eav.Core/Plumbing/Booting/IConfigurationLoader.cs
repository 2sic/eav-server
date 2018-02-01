namespace ToSic.Eav.Plumbing.Booting
{
    /// <summary>
    /// This is just a marker interface, to later on find the correct IoC configuration
    /// </summary>
    public interface IConfigurationLoader
    {
        void Configure();
    }
}
