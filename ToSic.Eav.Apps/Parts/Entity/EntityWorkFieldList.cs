using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Apps.Parts.Tools;
using ToSic.Eav.Data;
using Callback = System.Func<ToSic.Eav.Apps.Parts.Tools.CoupledIdLists, System.Collections.Generic.Dictionary<string, object>>;


namespace ToSic.Eav.Apps.Parts
{
    public class EntityWorkFieldList : AppWorkBase<IAppWorkCtxWithDb>
    {
        private readonly AppWork _appWork;

        public EntityWorkFieldList(AppWork appWork) : base("AWk.EntCre")
        {
            ConnectServices(
                _appWork = appWork
            );
        }

        public void FieldListUpdate(IEntity target, string[] fields, bool asDraft, Callback callback)
        {
            target = AppWorkCtx.AppState.GetDraftOrKeep(target);
            var lists = new CoupledIdLists(fields.ToDictionary(f => f, f => FieldListIdsWithNulls(target.Children(f))), Log);
            var values = callback.Invoke(lists);
            _appWork.EntityUpdate(AppWorkCtx)
            /*Parent.Entities*/.UpdatePartsFromValues(target, values, (published: !asDraft, branch: asDraft));
        }

        public void FieldListAdd(IEntity target, string[] fields, int index, int?[] values, bool asDraft, bool forceAddToEnd, bool padWithNulls = false)
            => FieldListUpdate(target, fields, asDraft, lists =>
            {
                // hitting + if the list is empty first add 1 null item (because we already see one demo item)
                // fix https://github.com/2sic/2sxc/issues/2943 
                if (lists.Lists.First().Value.Count < index && padWithNulls) // on non, first add 1 null item
                    lists.Add(0, new int?[] { null, null });

                return lists.Add(forceAddToEnd ? null : (int?)index, values);
            });

        public void FieldListRemove(IEntity target, string[] fields, int index, bool asDraft)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Remove(index));

        public void FieldListMove(IEntity target, string[] fields, int source, int dest, bool asDraft)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Move(source, dest));

        public void FieldListReorder(IEntity target, string[] fields, int[] newSequence, bool asDraft)
            => FieldListUpdate(target, fields, asDraft, lists => lists.Reorder(newSequence));


        public void FieldListReplaceIfModified(IEntity target, string[] fields, int index, int?[] replacement,
            bool asDraft)
            => FieldListUpdate(target, fields, asDraft,
                lists => lists.Replace(index, replacement.Select(r => (true, r)).ToArray()
                ));

        private static List<int?> FieldListIdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

    }
}
