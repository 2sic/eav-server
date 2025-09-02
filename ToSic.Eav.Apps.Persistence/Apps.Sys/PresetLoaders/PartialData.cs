namespace ToSic.Eav.Apps.Sys.PresetLoaders;

public record PartialData(ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities);
