using ToSic.Eav.Models;

namespace ToSic.Eav.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeNameId)]
public record ExportDecorator : ModelOfEntity
{
    public const string ContentTypeNameId = "32698880-1c2e-41ab-bcfc-420091d3263f";
    public const string ContentTypeName = "SystemExportDecorator";

    public bool IsContentType => Entity.MetadataFor.TargetType == (int) TargetTypes.ContentType;

    public bool IsEntity => Entity.MetadataFor.TargetType == (int)TargetTypes.Entity;

    public string? KeyString => Entity.MetadataFor.KeyString;

    public Guid? KeyGuid => Entity.MetadataFor.KeyGuid;

}