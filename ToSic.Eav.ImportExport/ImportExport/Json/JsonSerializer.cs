using ToSic.Eav.Data.Build;
using ToSic.Eav.Serialization.Internal;
using ToSic.Lib.DI;

namespace ToSic.Eav.ImportExport.Json;

partial class JsonSerializer: SerializerBase, IDataDeserializer
{
    public const string ReadOnlyMarker = "~";
    public const string NoLanguage = "*";

    #region Serializer Dependencies

    public new class MyServices: SerializerBase.MyServices
    {
        public MyServices(ITargetTypes metadataTargets, IAppStates appStates, DataBuilder dataBuilder, LazySvc<IValueConverter> valueConverter)
            : base(metadataTargets, dataBuilder, appStates)
        {
            ConnectLogs([
                ValueConverter = valueConverter
            ]);
        }
        public LazySvc<IValueConverter> ValueConverter { get; }
    }

    #endregion

    /// <summary>
    /// Constructor for DI
    /// </summary>
    public JsonSerializer(MyServices services) : this(services, "Jsn.Serlzr") {}
        

    /// <summary>
    /// Initialize with the correct logger name
    /// </summary>
    protected JsonSerializer(MyServices services, string logName): base(services, logName)
    {
    }

    /// <summary>
    /// WIP test API to ensure content-types serialized for UI resolve any hyperlinks.
    /// This is ATM only relevant to ensure that file-references in the WYSIWYG CSS work
    /// See https://github.com/2sic/2sxc/issues/2930
    /// Otherwise it's not used, so don't publish this API anywhere
    /// </summary>
    [PrivateApi]
    public bool ValueConvertHyperlinks = false;
}

internal static class StringHelpers
{
    public static string EmptyFallback(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
}