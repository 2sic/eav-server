using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.ImportExport.Sys.Xml;

internal record TargetLanguageToSourceLanguage : DimensionDefinition
{
    public ICollection<DimensionDefinition> PrioritizedDimensions = [];
}