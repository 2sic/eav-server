using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Global;

namespace ToSic.Eav.ImportExport.Json.Sys;

internal class JsonDeserializeEntityHelper(IGlobalDataService globalData, ContentTypeAssemblyKit contentTypeAssemblyKit, IJsonDataReader.Options options, ILog? logDsDetails)
{
    private IAppReader AppReaderOrErrorLazy => field ??= options.AppReaderOrNull ?? throw new ArgumentNullException(nameof(options.AppReaderOrNull), "AppReader must be provided to get content types");

    public IContentType? GetContentType(string staticName)
    {
        var l = logDsDetails.Fn<IContentType?>($"name: {staticName}, preferLocal: {options.PreferLocalAppTypes}", timer: true);
        // There is a complex lookup we must protocol to better detect issues, which is why we assemble a message
        var msg = "";

        // If local type is preferred, use the App accessor,
        // this will also check the global types internally
        // ReSharper disable once InvertIf
        if (options.PreferLocalAppTypes)
        {
            var type = AppReaderOrErrorLazy.TryGetContentType(staticName);
            if (type != null)
                return l.Return(type, $"app: found");
            msg += "app: not found, ";
        }

        var globalType = globalData.GetContentTypeIfAlreadyLoaded(staticName);

        if (globalType != null)
            return l.Return(globalType, $"{msg}global: found");
        msg += "global: not found, ";

        if (options.Types != null)
        {
            msg += "local-list: ";
            globalType = options.Types.FirstOrDefault(t => t.NameId == staticName);
        }
        else
        {
            msg += "app: ";
            globalType = AppReaderOrErrorLazy.TryGetContentType(staticName);
        }

        return l.Return(globalType, $"{msg}{(globalType == null ? "not " : "")}found");
    }

    public IContentType GetTransientContentType(string name, string nameId)
    {
        var transientContentType = contentTypeAssemblyKit.Type.Transient(options.AppId, name, nameId);
        return options.DeserializationSettings?.ContentTypeProvider?.LazyTypeGenerator(options.AppId, name, nameId, transientContentType)
               ?? transientContentType;
    }

}