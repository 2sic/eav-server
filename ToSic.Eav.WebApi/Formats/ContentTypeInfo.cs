using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.WebApi.Formats
{
    public class ContentTypeInfo: IHasExternalI18n
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string StaticName { get; set; }
        public string Scope { get; set; }
        public string Description { get; set; }
        public bool UsesSharedDef { get; set; }
        public int? SharedDefId { get; set; }
        public int Items { get; set; }
        public int Fields { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public string DebugInfoRepositoryAddress { get; set; }

        // ReSharper disable once InconsistentNaming
        public string I18nKey { get; set; }
    }

}