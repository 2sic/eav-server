namespace ToSic.Eav.Interfaces
{
    // ReSharper disable once InconsistentNaming
    public interface IHasExternalI18n
    {
        /// <summary>
        /// The key to provide internationalization for this object
        /// </summary>
        // ReSharper disable once InconsistentNaming
        string I18nKey { get; }
    }
}
