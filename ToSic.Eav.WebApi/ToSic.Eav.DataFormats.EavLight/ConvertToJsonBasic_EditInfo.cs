using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.WebApi.Dto;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight;

public partial class ConvertToEavLight
{

    private static void AddStatistics(IEntity entity, IDictionary<string, object> entityValues)
    {
        try
        {
            entityValues.Add("_Used", entity.Parents().Count);
            entityValues.Add("_Uses", entity.Children().Count);
            entityValues.Add("_Permissions", new { Count = entity.Metadata.Permissions.Count() });
        }
        catch { /* ignore */ }
    }

    private static void AddEditInfo(IEntity entity, IDictionary<string, object> entityValues)
    {
        try
        {
            entityValues.Add("_EditInfo", new EditInfoDto(entity));
        }
        catch { /* ignore */ }
    }
}