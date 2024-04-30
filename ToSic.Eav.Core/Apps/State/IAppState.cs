using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.Specs;
using ToSic.Eav.Metadata;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

public interface IAppState: IAppSpecs, IAppDataAndMetadataService, IMetadataSource
{

}