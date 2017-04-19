using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class DimensionsRepo: EfcRepoPart
    {
        public DimensionsRepo(EfcRepository parent) : base(parent) { }


        #region Cached Dimensions - important for import-scenarios where a lot of dimension access happens

        private static List<ToSicEavDimensions> _cachedDimensions;


        internal void EnsureDimensionsCache()
        {
            if (_cachedDimensions == null)
                _cachedDimensions = Db.ToSicEavDimensions.Include(d => d.ParentNavigation).ToList();
        }

        /// <summary>
        /// Clear DimensionsCache in current Application Cache
        /// </summary>
        private void ClearDimensionsCache()
        {
            _cachedDimensions = null;
        }

        #endregion

        private int GetDimensionId(string systemKey, string externalKey)
        {
            EnsureDimensionsCache();

            return _cachedDimensions.Where(d => string.Equals(d.SystemKey, systemKey, StringComparison.InvariantCultureIgnoreCase) && string.Equals(d.ExternalKey, externalKey, StringComparison.InvariantCultureIgnoreCase) && d.ZoneId == Parent.ZoneId).Select(d => d.DimensionId).FirstOrDefault();
        }


        private List<ToSicEavDimensions> GetLanguages() => GetDimensionChildren("Culture");

        private List<ToSicEavDimensions> GetDimensionChildren(string systemKey)
        {
            EnsureDimensionsCache();

            return _cachedDimensions.Where(d => d.Parent.HasValue && d.ParentNavigation.SystemKey == systemKey && d.ZoneId == Parent.ZoneId).ToList();
        }


        public void AddOrUpdateLanguage(string cultureCode, string cultureText, bool active)
        {
            var eavLanguage = GetLanguages().FirstOrDefault(l => l.ExternalKey == cultureCode);
            // If the language exists in EAV, set the active state, else add it
            if (eavLanguage != null)
                UpdateDimension(eavLanguage.DimensionId, active);
            else
                AddLanguage(cultureText, cultureCode);

        }

        /// <summary>
        /// Update a single Dimension
        /// </summary>
        private void UpdateDimension(int dimensionId, bool? active = null)//, string name = null)
        {
            var dimension = Db.ToSicEavDimensions.Single(d => d.DimensionId == dimensionId);
            if (active.HasValue)
                dimension.Active = active.Value;

            Db.SaveChanges();
            ClearDimensionsCache();
        }

        /// <summary>
        /// Add a new Language to current Zone
        /// </summary>
        private void AddLanguage(string name, string externalKey)
        {
            var newLanguage = new ToSicEavDimensions
            {
                Name = name,
                ExternalKey = externalKey,
                Parent = GetDimensionId("Culture", null),
                ZoneId = Parent.ZoneId
            };
            Db.ToSicEavDimensions.Add(newLanguage);
            Db.SaveChanges();
            ClearDimensionsCache();
        }


        /// <summary>
        /// Add a new Dimension
        /// </summary>
        internal void AddDimension(string systemKey, string name, ToSicEavZones zone, ToSicEavDimensions parent = null)//, bool autoSave = false)
        {
            var newDimension = new ToSicEavDimensions()
            {
                SystemKey = systemKey,
                Name = name,
                Zone = zone,
                ParentNavigation = parent
            };
            Db.ToSicEavDimensions.Add(newDimension);

            //if (autoSave)
            //    Db.SaveChanges();

            ClearDimensionsCache();
        }

    }
}
