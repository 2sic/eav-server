using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Serialization;
using ToSic.Eav.Metadata;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.ImportExport.Json
{
    public partial class JsonSerializer: SerializerBase, IDataDeserializer
    {
        public const string ReadOnlyMarker = "~";
        public const string NoLanguage = "*";

        #region Serializer Dependencies

        public new class Dependencies: SerializerBase.Dependencies
        {
            public Dependencies(ITargetTypes metadataTargets, IAppStates appStates, MultiBuilder multiBuilder, LazySvc<IValueConverter> valueConverter)
                : base(metadataTargets, appStates)
            {
                AddToLogQueue(
                    MultiBuilder = multiBuilder,
                    ValueConverter = valueConverter
                );
            }

            public MultiBuilder MultiBuilder { get; }
            public LazySvc<IValueConverter> ValueConverter { get; }
        }

        #endregion

        /// <summary>
        /// Constructor for DI
        /// </summary>
        public JsonSerializer(Dependencies dependencies) : this(dependencies, "Jsn.Serlzr") {}
        

        /// <summary>
        /// Initialize with the correct logger name
        /// </summary>
        protected JsonSerializer(Dependencies dependencies, string logName): base(dependencies, logName)
        {
            MultiBuilder = dependencies.MultiBuilder;
        }
        [PrivateApi]
        protected MultiBuilder MultiBuilder { get; }

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
        public static string EmptyAlternative(this string s, string alternative) => string.IsNullOrEmpty(s) ? alternative : s;
    }
}
