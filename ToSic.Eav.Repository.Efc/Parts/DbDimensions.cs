using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
	public class DbDimensions: BllCommandBase
	{
        public DbDimensions(DbDataController ctx) : base(ctx) { }
        private static List<ToSicEavDimensions> _cachedDimensions;

        #region Cached Dimensions



	    internal void EnsureDimensionsCache()
        {
            if (_cachedDimensions != null) return;
            _cachedDimensions = DbContext.SqlDb.ToSicEavDimensions.ToList();
        }

        /// <summary>
        /// Clear DimensionsCache in current Application Cache
        /// </summary>
        public void ClearDimensionsCache()
        {
            _cachedDimensions = null;
        }

        #endregion

		internal int GetDimensionId(string systemKey, string externalKey)
		{
			EnsureDimensionsCache();

		    return _cachedDimensions.Where(d =>
		            string.Equals(d.SystemKey, systemKey, StringComparison.InvariantCultureIgnoreCase)
		            && string.Equals(d.ExternalKey, externalKey, StringComparison.InvariantCultureIgnoreCase)
		            && d.ZoneId == DbContext.ZoneId)
		        .Select(d => d.DimensionId).FirstOrDefault();
		}

		/// <summary>
		/// Get a single Dimension
		/// </summary>
		/// <returns>A Dimension or null</returns>
		public ToSicEavDimensions GetDimension(int dimensionId) 
            => DbContext.SqlDb.ToSicEavDimensions.SingleOrDefault(d => d.DimensionId == dimensionId && d.ZoneId == DbContext.ZoneId);

	    /// <summary>
		/// Get Dimensions by Ids
		/// </summary>
		internal List<ToSicEavDimensions> GetDimensions(IEnumerable<int> dimensionIds) 
            => DbContext.SqlDb.ToSicEavDimensions.Where(d => dimensionIds.Contains(d.DimensionId) && d.ZoneId == DbContext.ZoneId).ToList();

	    /// <summary>
		/// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
		/// </summary>
		private List<ToSicEavDimensions> GetDimensionChildren(string systemKey, bool includeInactive)
		{
			EnsureDimensionsCache();

		    return _cachedDimensions.Where(d =>
		        d.Parent.HasValue
		        && d.ParentNavigation.SystemKey == systemKey
		        && d.ZoneId == DbContext.ZoneId
                && (includeInactive || d.Active)
                ).ToList();
		}

		/// <summary>
		/// Update a single Dimension
		/// </summary>
		private void UpdateDimension(int dimensionId, bool? active = null, string name = null)
		{
			var dimension = DbContext.SqlDb.ToSicEavDimensions.Single(d => d.DimensionId == dimensionId);
			if (active.HasValue)
				dimension.Active = active.Value;
			if (name != null)
				dimension.Name = name;

            DbContext.SqlDb.SaveChanges();
			ClearDimensionsCache();
		}

        public void AddOrUpdateLanguage(string cultureCode, string cultureText, bool active)
        {
            var eavLanguage = GetLanguages(true).FirstOrDefault(l => l.ExternalKey == cultureCode);
            // If the language exists in EAV, set the active state, else add it
            if (eavLanguage != null)
                UpdateDimension(eavLanguage.DimensionId, active);
            else
                AddLanguage(cultureText, cultureCode);
        }




        /// <summary>
        /// Add a new Dimension
        /// </summary>
        internal void AddDimension(string systemKey, string name, ToSicEavZones zone, ToSicEavDimensions parent = null)
		{
			var newDimension = new ToSicEavDimensions
            {
				SystemKey = systemKey,
				Name = name,
				Zone = zone,
				ParentNavigation = parent
			};
            DbContext.SqlDb.Add(newDimension);
            DbContext.SqlDb.SaveChanges();

			ClearDimensionsCache();
		}

		#region Languages

		/// <summary>
		/// Get all Languages of current Zone and App
		/// </summary>
		public List<ToSicEavDimensions> GetLanguages(bool includeInactive = false) => GetDimensionChildren(Constants.CultureSystemKey, includeInactive);


        /// <summary>
        /// Generate a language list which will have at least 1 language in it for import-purposes
        /// </summary>
        /// <param name="defaultLanguage"></param>
        /// <returns></returns>
	    public List<Dimension> GetLanguageListForImport(string defaultLanguage)
	    {
            var langs = GetLanguages();
            if (langs.Count == 0)
                langs.Add(new ToSicEavDimensions
                {
                    Active = true,
                    ExternalKey = defaultLanguage,
                    Name = "(added by import System, default language " + defaultLanguage + ")",
                    SystemKey = Constants.CultureSystemKey
                });
            return langs.Select(d => new Dimension { DimensionId = d.DimensionId, Key = d.ExternalKey }).ToList();
        }

        public string[] GetLanguagesExtNames() => GetLanguages().Select(language => language.ExternalKey).ToArray();

        /// <summary>
        /// Add a new Language to current Zone
        /// </summary>
        private void AddLanguage(string name, string externalKey)
		{
			var newLanguage = new ToSicEavDimensions
            {
				Name = name,
				ExternalKey = externalKey,
				Parent = GetDimensionId(Constants.CultureSystemKey, null),
                ZoneId = DbContext.ZoneId,
                Active = true
			};
            DbContext.SqlDb.Add(newLanguage);
            DbContext.SqlDb.SaveChanges();
			ClearDimensionsCache();
		}
        
	    #endregion
	}
}
