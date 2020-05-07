using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.Parts.Tools;
using ToSic.Eav.Data;
using UpdateList = System.Collections.Generic.Dictionary<string, object>;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public void FieldListUpdate(IEntity target, string[] fields, bool enableVersioning,
            Func<CoupledIdLists, UpdateList> callback)
        {
            var lists = new CoupledIdLists(fields.ToDictionary(f => f, f => FieldListIdsWithNulls(target.Children(f))), Log);
            var values = callback.Invoke(lists);
            AppManager.Entities.UpdateParts(target, values, enableVersioning);
        }

        public void FieldListRemove(IEntity target, string[] fields, int index, bool enableVersioning)
            => FieldListUpdate(target, fields, enableVersioning, lists => lists.Remove(index));

        public void FieldListMove(IEntity target, string[] fields, int source, int dest, bool enableVersioning) 
            => FieldListUpdate(target, fields, enableVersioning, lists => lists.Move(source, dest));

        public void FieldListReorder(IEntity target, string[] fields, int[] newSequence, bool enableVersioning)
            => FieldListUpdate(target, fields, enableVersioning, lists => lists.Reorder(newSequence));


        public void FieldListReplaceIfModified(IEntity target, string[] fields, int index, int?[] replacement, bool enableVersioning)
        {
            FieldListUpdate(target, fields, enableVersioning, 
                lists => lists.Replace(index, replacement.Select(r => new Tuple<bool, int?>(true, r)).ToArray()
            ));
        }

        private static List<int?> FieldListIdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

    }
}
