namespace ToSic.Eav.Interfaces
{
    // ReSharper disable once InconsistentNaming
    public interface IHasExternalI18n
    {
        // 2019-11-12 2dm - I believe this was never really used, but I'm not sure

        /// <summary>
        /// The key to provide internationalization for this object
        /// </summary>
        // ReSharper disable once InconsistentNaming
        string I18nKey { get; }
    }
}
