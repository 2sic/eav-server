namespace ToSic.Eav.Data.Publishing.Sys;

/// <summary>
/// This service will check environmental requirements regarding publishing rules. 
/// </summary>
/// <remarks>
/// As of now it's only properly implemented in DNN, where the page can have publishing workflows attached.
/// Those rules can force the data to be saved as draft or similar.
/// But since various data updates require publishing context, it's moved to Eav.
/// </remarks>
public interface IDataPublishingService
{
    /// <summary>
    /// Wraps an action and performs pre/post processing related to versioning of the environment.
    /// </summary>
    /// <param name="action"></param>
    void DoInsidePublishing(Action<PublishingOptions> action);
}
