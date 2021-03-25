using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.Conversion
{
    public abstract partial class EntitiesToDictionaryBase
    {

        private static void AddStatistics(IEntity entity, Dictionary<string, object> entityValues)
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
