using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Metadata;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

public interface IAppState: IAppSpecsWithState, IAppDataAndMetadataService, IMetadataSource, IAppSpecsWithStateAndCache;