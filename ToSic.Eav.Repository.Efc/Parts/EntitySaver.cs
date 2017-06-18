using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class EntitySaver
    {
        /// <summary>
        /// Goal: Pass changes into an existing entity so that it can then be saved as a whole, with correct
        /// modifications. 
        /// </summary>
        /// <param name="origE"></param>
        /// <param name="newE"></param>
        /// <param name="ctToDo"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        public static IEntity CreateMergedForSaving(IEntity origE, IEntity newE, IContentType ctToDo, SaveOptions saveOptions)
        {
            #region initial error checks
            if(newE?.Attributes?.Count == 0)
                throw new Exception("can't prepare entities for saving, no new item with attributes provided");

            #endregion

            #region check original item
            // only accept original if it's a real object with a valid GUID, otherwise it's not an existing entity
            bool hasOriginal = !(origE == null || (origE.EntityId == 0 && origE.EntityGuid == Guid.Empty));
            var idProvidingEntity = hasOriginal ? origE : newE;
            #endregion

            #region clean up unwanted attributes - then merge into a final attribute list

            var origAttribs = origE?.Attributes;
            var newAttribs = newE.Attributes;

            // Optionally remove original values not in the update
            if (hasOriginal && !saveOptions.PreserveExistingAttributes)
                origAttribs = KeepOnlyKnownKeys(origAttribs, newE.Attributes.Keys.ToList());

            // Optionaly remove unknown - if possible - of both original and new
            if (saveOptions.RemoveUnknownAttributes && ctToDo != null)
            {
                var keys = ctToDo.Attributes.Select(a => a.Name).ToList();
                if(hasOriginal) origAttribs = KeepOnlyKnownKeys(origAttribs, keys);
                newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
            }

            // now merge into new target
            var mergedAttribs = hasOriginal ? origAttribs.Copy() : newAttribs; // will become 
            if (hasOriginal)
                foreach (var newAttrib in newAttribs)
                    mergedAttribs[newAttrib.Key] = newAttrib.Value;

            #endregion

            return new Entity(idProvidingEntity, mergedAttribs, null);
        }

        private static Dictionary<string, IAttribute> KeepOnlyKnownKeys(Dictionary<string, IAttribute> orig, List<string> keys)
        {
            var lowerKeys = keys.Select(k => k.ToLowerInvariant()).ToList();
            return orig.Where(a => lowerKeys.Contains(a.Key.ToLowerInvariant()))
                .ToDictionary(a => a.Key, a => a.Value);
        }

    }

    public class SaveOptions
    {
        //public SaveTypes Mode = SaveTypes.Update;
        public bool PreserveExistingAttributes = false;
        //public bool PreserveInvisibleAttributes = false;
        public bool PreserveOtherLanguages = false;
        public bool RemoveUnknownAttributes = true;
    }

    //public enum SaveTypes
    //{
    //    New,
    //    Update,
    //    Delete
    //}
}
