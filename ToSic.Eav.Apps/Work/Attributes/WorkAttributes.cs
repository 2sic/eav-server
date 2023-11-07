using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Work
{
    public class WorkAttributes : WorkUnitBase<IAppWorkCtx>
    {

        public WorkAttributes() : base("ApS.InpGet")
        {
        }

        public List<PairTypeWithAttribute> GetFields(string staticName)
        {
            var l = Log.Fn<List<PairTypeWithAttribute>>($"get fields a#{AppWorkCtx.Show()}, type:{staticName}");

            if (!(AppWorkCtx.AppState.GetContentType(staticName) is IContentType type))
                return l.Return(new List<PairTypeWithAttribute>(),
                    $"error, type:{staticName} is null, it is missing or it is not a ContentType - something broke");

            var fields = type.Attributes.OrderBy(a => a.SortOrder);

            return l.Return(fields.Select(a => new PairTypeWithAttribute { Type = type, Field = a }).ToList());
        }

        public List<PairTypeWithAttribute> GetSharedFields()
        {
            var l = Log.Fn<List<PairTypeWithAttribute>>($"get shared fields {AppWorkCtx.Show()}");
            var appState = AppWorkCtx.AppState;
            var localTypes = appState.ContentTypes
                .Where(ct => !ct.HasAncestor())
                .ToList();

            var fields = localTypes
                .SelectMany(ct => ct.Attributes
                    .Where(a => a.Guid != null && a.SysSettings?.Share == true)
                    .Select(a => new PairTypeWithAttribute { Type = ct, Field = a }))
                .OrderBy(set => set.Type.Name)
                .ThenBy(set => set.Field.Name)
                .ToList();

            return l.Return(fields);
        }
    }
}
