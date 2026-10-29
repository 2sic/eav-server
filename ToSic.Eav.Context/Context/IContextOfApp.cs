using ToSic.Eav.Apps;
using ToSic.Eav.Data.Sys.PropertyStack;

namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IContextOfApp: IContextOfSite
{
    /// <summary>
    /// The App settings (if the app was available)
    /// </summary>
    PropertyStack AppSettings { get; }

    /// <summary>
    /// The App resources (if the app was available)
    /// </summary>
    PropertyStack AppResources { get; }

    /// <summary>
    /// Optional AppReader - to check if the context really has something.
    /// </summary>
    IAppReader? AppReaderOrNull { get; }

    /// <summary>
    /// Full app reader - or exception if accessed incorrectly.
    /// </summary>
    IAppReader AppReaderRequired { get; }
}