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

            #region clean up unwanted attributes

            var origAttribs = origE?.Attributes.Copy();
            var newAttribs = newE.Attributes;

            // Optionally remove original values not in the update
            if (hasOriginal && !saveOptions.PreserveExistingAttributes)
                origAttribs = KeepOnlyKnownKeys(origAttribs, newE.Attributes.Keys.ToList());

            // Optionaly remove unknown - if possible - of both original and new
            if (!saveOptions.PreserveUnknownAttributes && ctToDo != null)
            {
                var keys = ctToDo.Attributes.Select(a => a.Name).ToList();
                if(hasOriginal) origAttribs = KeepOnlyKnownKeys(origAttribs, keys);
                newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
            }

            #endregion

            #region clear languages as needed

            // pre check if languages are properly available for clean-up or merge
            if(!saveOptions.PreserveUnknownLanguages)
                if ((!saveOptions.Languages?.Any() ?? true)
                    || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                    || saveOptions.Languages.All(l => l.Key != saveOptions.PrimaryLanguage))
                    throw new Exception("primary language must exist in languages, cannot continue preparation to save with unclear language setup");


            if (!saveOptions.PreserveUnknownLanguages && (saveOptions.Languages?.Any() ?? false))
            {
                if (hasOriginal) StripUnknownLanguages(origAttribs, saveOptions);
                StripUnknownLanguages(newAttribs, saveOptions);
            }

            #endregion

            // now merge into new target
            Dictionary<string, IAttribute> mergedAttribs = hasOriginal ? origAttribs : newAttribs; // will become 
            if(hasOriginal)
                foreach (var newAttrib in newAttribs)
                    mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages
                        ? MergeAttribute(mergedAttribs[newAttrib.Key], newAttrib.Value, saveOptions)
                        : newAttrib.Value;

            return new Entity(idProvidingEntity, mergedAttribs, null);
        }

        /// <summary>
        /// Will remove all language-information for values which have no language
        /// </summary>
        /// <param name="attribs"></param>
        /// <param name="saveOptions"></param>
        /// <remarks>
        /// this expects that saveOptions contain Languages & PrimaryLanguage, and that this is reliable
        /// </remarks>
        private static void StripUnknownLanguages(Dictionary<string, IAttribute> attribs, SaveOptions saveOptions)
        {
            var languages = saveOptions.Languages;

            foreach (var attribElm in attribs)
            {
                var values = new List<IValue>();       // new empty values list

                // when we go through the values, we should always take the primary language first
                // this is detectable by having either no language, or having the primary language
                var valuesWithPrimaryFirst = attribElm.Value.Values
                    .OrderBy(v => 
                    v.Languages == null || !v.Languages.Any() ? 2 // no language, so it's primary
                    : v.Languages.Any(l => l.Key == saveOptions.PrimaryLanguage) 
                        ? 1 // really primary, use this first
                        : 3); // other, work on these last
                foreach (var value in valuesWithPrimaryFirst)
                {
                    // create filtered list of languages
                    var newLangs = value.Languages?.Where(l => languages.Any(sysLang => sysLang.Key == l.Key)).ToList();
                    // only keep this value, if it is either the first (so contains primary or null-language) or that it still has a remaining language assignment
                    if (!values.Any() || (newLangs?.Any() ?? false))
                    {
                        value.Languages = newLangs;
                        values.Add(value);
                    }
                }
                attribElm.Value.Values = values;
            }
        }

        /// <summary>
        /// Merge two attributes, preserving languages as necessary
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        private static IAttribute MergeAttribute(IAttribute original, IAttribute update, SaveOptions saveOptions)
        {
            return original;
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
        public bool PreserveUnknownAttributes = false;
        //public bool PreserveInvisibleAttributes = false;

        public string PrimaryLanguage = null;
        public List<ILanguage> Languages = null;
        public bool PreserveExistingLanguages = false;
        public bool PreserveUnknownLanguages = false;

    }

    //public enum SaveTypes
    //{
    //    New,
    //    Update,
    //    Delete
    //}
}
