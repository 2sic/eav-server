using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps;

public interface IMetadataOfSource
{
    public IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title = null);
}