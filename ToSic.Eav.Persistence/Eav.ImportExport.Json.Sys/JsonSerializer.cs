using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.Global;
using ToSic.Eav.Data.Sys.ValueConverter;
using ToSic.Eav.Metadata.Targets;
using ToSic.Eav.Serialization.Sys;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonSerializer(JsonSerializer.Dependencies services, string logName = "Jsn.Serlzr"): SerializerBase(services, logName), IDataDeserializer
{
    public const string ReadOnlyMarker = "~";
    public const string NoLanguage = "*";

    #region Serializer Dependencies / MyServices

    public new record Dependencies(
        ITargetTypeService MetadataTargets,
        IGlobalDataService GlobalData,
        DataBuilder DataBuilder,
        LazySvc<IValueConverter> ValueConverter)
        : SerializerBase.Dependencies(MetadataTargets, DataBuilder, GlobalData, Connect: [ValueConverter]);

    #endregion

    /// <summary>
    /// WIP test API to ensure content-types serialized for UI resolve any hyperlinks.
    /// This is ATM only relevant to ensure that file-references in the WYSIWYG CSS work
    /// See https://github.com/2sic/2sxc/issues/2930
    /// Otherwise it's not used, so don't publish this API anywhere
    /// </summary>
    [PrivateApi]
    public bool ValueConvertHyperlinks = false;

}
