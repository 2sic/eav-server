using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Sys.Xml;

internal record TargetLanguageToSourceLanguage : DimensionDefinition
{
    public ICollection<DimensionDefinition> PrioritizedDimensions = [];
}