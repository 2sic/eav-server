
namespace ToSic.Eav.Internal.Features;

[PrivateApi("Internal stuff only")]
public interface ILibFeaturesService
{
    bool IsEnabled(string nameIds);
}