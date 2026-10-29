namespace ToSic.Eav.Apps.Sys.PresetLoaders;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record PartialData(ICollection<IContentType> ContentTypes, ICollection<IEntity> Entities);
