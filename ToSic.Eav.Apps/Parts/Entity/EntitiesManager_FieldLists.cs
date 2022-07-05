using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api.Api01;
using ToSic.Eav.Apps.Parts.Tools;
using ToSic.Eav.Data;
using Callback = System.Func<ToSic.Eav.Apps.Parts.Tools.CoupledIdLists, System.Collections.Generic.Dictionary<string, object>>;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public void FieldListUpdate(IEntity target, string[] fields, bool asDraft, Callback callback)
        {
            var lists = new CoupledIdLists(fields.ToDictionary(f => f, f => FieldListIdsWithNulls(target.Children(f))), Log);
            var values = callback.Invoke(lists);
            Parent.Entities.UpdatePartsFromValues(target, values, SimpleDataController.DraftAndBranch(asDraft));
        }

        public void FieldListAdd(IEntity target, string[] fields, int index, int?[] values, bool asDraft, bool forceAddToEnd)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Add(forceAddToEnd ? null : (int?)index, values));
        
        public void FieldListRemove(IEntity target, string[] fields, int index, bool asDraft)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Remove(index));

        public void FieldListMove(IEntity target, string[] fields, int source, int dest, bool asDraft) 
            => FieldListUpdate(target, fields, asDraft, lists => lists.Move(source, dest));

        public void FieldListReorder(IEntity target, string[] fields, int[] newSequence, bool asDraft)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Reorder(newSequence));


        public void FieldListReplaceIfModified(IEntity target, string[] fields, int index, int?[] replacement,
            bool asDraft)
            => FieldListUpdate(target, fields, asDraft,
                lists => lists.Replace(index, replacement.Select(r => new Tuple<bool, int?>(true, r)).ToArray()
                ));

        private static List<int?> FieldListIdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

    }
}
