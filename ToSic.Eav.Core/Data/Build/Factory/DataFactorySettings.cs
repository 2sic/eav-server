using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build
{
    public class DataFactorySettings
    {
        /// <summary>
        /// The App-ID which will be assigned to the generated entities.
        /// By default it will be `0`
        /// </summary>
        public int AppId { get; }
        public string TypeName { get; }

        /// <summary>
        /// The field in the data which is the default title.
        /// Defaults to `Title` if not set.
        /// </summary>
        public string TitleField { get; }

        /// <summary>
        /// Determines if Zero IDs are auto-incremented - default is `true`.
        /// </summary>
        public bool AutoId { get; }

        public int IdSeed { get; }

        public DataFactorySettings(
            DataFactorySettings original = default,
            string noParamOrder = Eav.Parameters.Protector,
            int? appId = default,
            string typeName = default,
            string titleField = default,
            bool? autoId = default,
            int? idSeed = default
        )
        {
            AppId = appId ?? original?.AppId ?? 0;
            TypeName = typeName ?? original?.TypeName ?? DataConstants.DataFactoryDefaultTypeName;
            TitleField = titleField.UseFallbackIfNoValue(original?.TitleField).UseFallbackIfNoValue(Attributes.TitleNiceName);
            AutoId = autoId ?? original?.AutoId ?? true;
            IdSeed = idSeed ?? original?.IdSeed ?? 1;
        }
    }
}
