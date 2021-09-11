using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    public partial class ConvertToJsonBasic
    {

        private static void AddStatistics(IEntity entity, IDictionary<string, object> entityValues)
        {
            try
            {
                entityValues.Add("_Used", entity.Parents().Count);
                entityValues.Add("_Uses", entity.Children().Count);
                entityValues.Add("_Permissions", new { Count = entity.Metadata.Permissions.Count() });
            }
            catch
            {
                /* ignore */
            }
        }


    }
}
