using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Sys.Xml;

[PrivateApi]
public record TargetLanguageToSourceLanguage : DimensionDefinition
{
    public ICollection<DimensionDefinition> PrioritizedDimensions = [];
}