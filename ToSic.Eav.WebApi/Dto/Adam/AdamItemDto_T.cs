using System;

namespace ToSic.Eav.WebApi.Dto;

public class AdamItemDto<TFolderId, TItemId>: AdamItemDto
{
    public TItemId Id { get; set; }
    public TFolderId ParentId { get; set; }

    public AdamItemDto(bool isFolder, TItemId id, TFolderId parentId, string name, int size, DateTime created, DateTime modified)
        :base(isFolder, name, size, created, modified)
    {
        Id = id;
        ParentId = parentId;
    }
}