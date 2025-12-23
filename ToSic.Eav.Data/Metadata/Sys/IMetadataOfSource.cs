namespace ToSic.Eav.Metadata.Sys;

public interface IMetadataOfSource
{
    /// <summary>
    /// Gets MetadataOf objects containing a list of Metadata + target information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetType"></param>
    /// <param name="key"></param>
    /// <param name="npo">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="title"></param>
    /// <returns></returns>
    public IMetadata GetMetadataOf<T>(TargetTypes targetType, T key, NoParamOrder npo = default, string? title = default);
}