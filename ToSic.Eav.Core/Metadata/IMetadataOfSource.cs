using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

public interface IMetadataOfSource
{
    /// <summary>
    /// Gets MetadataOf objects containing a list of Metadata + target information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetType"></param>
    /// <param name="key"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title = null);
}