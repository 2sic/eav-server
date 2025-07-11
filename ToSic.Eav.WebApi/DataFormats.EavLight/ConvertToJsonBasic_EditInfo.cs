﻿using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.DataFormats.EavLight;

partial class ConvertToEavLight
{

    private static void AddStatistics(IEntity entity, IDictionary<string, object?> entityValues)
    {
        try
        {
            entityValues.Add("_Used", entity.Parents().Count());
            entityValues.Add("_Uses", entity.Children().Count());
            entityValues.Add("_Permissions", new { Count = entity.Metadata.Permissions.Count() });
        }
        catch { /* ignore */ }
    }

    private static void AddEditInfo(IEntity entity, IDictionary<string, object?> entityValues)
    {
        try
        {
            entityValues.Add("_EditInfo", new EditInfoDto(entity));
        }
        catch { /* ignore */ }
    }
}