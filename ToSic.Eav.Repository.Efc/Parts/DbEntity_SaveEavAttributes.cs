using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    partial class DbEntity
    {

        /// <summary>
        /// Remove values and attached dimensions of these values from the DB
        /// Important when updating json-entities, to ensure we don't keep trash around
        /// </summary>
        /// <param name="entityId"></param>
        private bool ClearAttributesInDbModel(int entityId)
        {
            var callLog = Log.Fn<bool>(timer: true);
            var val = DbContext.SqlDb.ToSicEavValues
                .Include(v => v.ToSicEavValuesDimensions)
                .Where(v => v.EntityId == entityId)
                .ToList();

            if (val.Count == 0) return callLog.ReturnFalse("no changes");

            var dims = val.SelectMany(v => v.ToSicEavValuesDimensions);
            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.RemoveRange(dims));
            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.RemoveRange(val));
            return callLog.ReturnTrue("ok");
        }

        private void SaveAttributesAsEav(IEntity newEnt,
            SaveOptions so,
            List<ToSicEavAttributes> dbAttributes,
            ToSicEavEntities dbEnt,
            int changeId,
            bool logDetails
        ) => Log.Do($"id:{newEnt.EntityId}", timer: true, action: () =>
        {
            if (!_attributeQueueActive) throw new Exception("Attribute save-queue not ready - should be wrapped");
            foreach (var attribute in newEnt.Attributes.Values)
                Log.Do($"InnerAttribute:{attribute.Name}", () =>
                {
                    // find attribute definition
                    var attribDef =
                        dbAttributes.SingleOrDefault(
                            a => string.Equals(a.StaticName, attribute.Name,
                                StringComparison.InvariantCultureIgnoreCase));
                    if (attribDef == null)
                    {
                        if (!so.DiscardAttributesNotInType)
                            throw new Exception(
                                $"trying to save attribute {attribute.Name} but can\'t find definition in DB");
                        return "attribute not found, will skip according to save-options";
                    }

                    if (attribDef.Type == ValueTypes.Entity.ToString())
                        return "type is entity, skip for now as relationships are processed later";

                    foreach (var value in attribute.Values)
                    {
                        #region prepare languages - has extensive error reporting, to help in case any db-data is bad

                        List<ToSicEavValuesDimensions> toSicEavValuesDimensions;
                        try
                        {
                            toSicEavValuesDimensions = value.Languages?.Select(l => new ToSicEavValuesDimensions
                            {
                                DimensionId = _zoneLangs.Single(ol => ol.Matches(l.Key)).DimensionId,
                                ReadOnly = l.ReadOnly
                            }).ToList();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                "something went wrong building the languages to save - " +
                                "your DB probably has some wrong language information which doesn't match; " +
                                "maybe even a duplicate entry for a language code" +
                                " - see https://github.com/2sic/2sxc/issues/1293",
                                ex);
                        }

                        #endregion

                        if (logDetails)
                            Log.A(Log.Try(() =>
                                $"add attrib:{attribDef.AttributeId}/{attribDef.StaticName} vals⋮{attribute.Values?.Count()}, dim⋮{toSicEavValuesDimensions?.Count}"));

                        var newVal = new ToSicEavValues
                        {
                            AttributeId = attribDef.AttributeId,
                            Value = value.Serialized ?? "",
                            ChangeLogCreated = changeId, // todo: remove some time later
                            ToSicEavValuesDimensions = toSicEavValuesDimensions,
                            EntityId = dbEnt.EntityId
                        };
                        AttributeQueueAdd(() => DbContext.SqlDb.ToSicEavValues.Add(newVal));
                    }

                    return "ok";
                });
        });

        internal void DoWhileQueueingAttributes(Action action)
        {
            var randomId = Guid.NewGuid().ToString().Substring(0, 4);
            Log.Do($"attribute queue:{randomId} start", () =>
            {
                if (_attributeUpdateQueue.Any())
                    throw new Exception("Attribute queue started while already containing stuff - bad!");
                _attributeQueueActive = true;
                // 1. check if it's the outermost call, in which case afterwards we import
                //var willPurgeQueue = _isOutermostCall;
                // 2. make sure any follow-up calls are not regarded as outermost
                //_isOutermostCall = false;
                // 3. now run the inner code
                action.Invoke();
                // 4. now check if we were the outermost call, in if yes, save the data
                DbContext.DoAndSaveWithoutChangeDetection(AttributeQueueRun);
                _attributeQueueActive = false;
            });
        }

        // ReSharper disable once RedundantDefaultMemberInitializer
        private bool _attributeQueueActive = false;
        private readonly List<Action> _attributeUpdateQueue = new List<Action>();

        private void AttributeQueueAdd(Action next) => _attributeUpdateQueue.Add(next);

        private void AttributeQueueRun() => Log.Do(timer: true, action: () =>
        {
            _attributeUpdateQueue.ForEach(a => a.Invoke());
            _attributeUpdateQueue.Clear();
        });
    }
}
