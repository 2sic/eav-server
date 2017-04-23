using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
	public class DbDimensions: BllCommandBase
	{
        public DbDimensions(DbDataController ctx) : base(ctx) { }
        private static List<ToSicEavDimensions> _cachedDimensions;

        #region Cached Dimensions



	    internal void EnsureDimensionsCache()
        {
            if (_cachedDimensions == null)
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

            return _cachedDimensions.Where(d => string.Equals(d.SystemKey, systemKey, StringComparison.InvariantCultureIgnoreCase) && string.Equals(d.ExternalKey, externalKey, StringComparison.InvariantCultureIgnoreCase) && d.ZoneId == DbContext.ZoneId).Select(d => d.DimensionId).FirstOrDefault();
		}

		/// <summary>
		/// Get a single Dimension
		/// </summary>
		/// <returns>A Dimension or null</returns>
		public ToSicEavDimensions GetDimension(int dimensionId) 
            => DbContext.SqlDb.ToSicEavDimensions.SingleOrDefault(d => d.DimensionId == dimensionId);

	    /// <summary>
		/// Get Dimensions by Ids
		/// </summary>
		internal IEnumerable<ToSicEavDimensions> GetDimensions(IEnumerable<int> dimensionIds) 
            => DbContext.SqlDb.ToSicEavDimensions.Where(d => dimensionIds.Contains(d.DimensionId) && d.ZoneId == DbContext.ZoneId);

	    /// <summary>
		/// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
		/// </summary>
		public List<ToSicEavDimensions> GetDimensionChildren(string systemKey)
		{
			EnsureDimensionsCache();

			return _cachedDimensions.Where(d => d.Parent.HasValue && d.ParentNavigation.SystemKey == systemKey && d.ZoneId == DbContext.ZoneId).ToList();
		}

		///// <summary>
		///// Test whehter Value exists on specified Entity and Attribute with specified DimensionIds 
		///// </summary>
		//public bool ValueExists(List<int> dimensionIds, int entityId, int attributeId) 
  //          => Context.SqlDb.Values.Any(v => !v.ChangeLogDeleted.HasValue && v.EntityId == entityId && v.AttributeId == attributeId && v.ValuesDimensions.All(d => dimensionIds.Contains(d.DimensionId)));

        // 2017-04-05 2dm clean-up
	 //   /// <summary>
		///// Get Dimensions to use for specified Entity and Attribute if unknown
		///// </summary>
		//public IQueryable<Dimension> GetFallbackDimensions(int entityId, int attributeId)
		//{
  //          return from vd in Context.SqlDb.ValuesDimensions
		//		   where
		//			  vd.Value.EntityId == entityId &&
		//			  vd.Value.AttributeId == attributeId &&
		//			  !vd.Value.ChangeLogDeleted.HasValue
		//		   orderby vd.Value.ChangeLogCreated
		//		   select
		//			  vd.Dimension;
		//}

		/// <summary>
		/// Update a single Dimension
		/// </summary>
		public void UpdateDimension(int dimensionId, bool? active = null, string name = null)
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
            var eavLanguage = GetLanguages().FirstOrDefault(l => l.ExternalKey == cultureCode);
            // If the language exists in EAV, set the active state, else add it
            if (eavLanguage != null)
                UpdateDimension(eavLanguage.DimensionId, active);
            else
                AddLanguage(cultureText, cultureCode);

        }




        /// <summary>
        /// Add a new Dimension
        /// </summary>
        internal void AddDimension(string systemKey, string name, ToSicEavZones zone, ToSicEavDimensions parent = null, bool autoSave = false)
		{
			var newDimension = new ToSicEavDimensions
            {
				SystemKey = systemKey,
				Name = name,
				Zone = zone,
				ParentNavigation = parent
			};
            DbContext.SqlDb.Add(newDimension);

			if (autoSave)
                DbContext.SqlDb.SaveChanges();

			ClearDimensionsCache();
		}

		#region Languages

		/// <summary>
		/// Get all Languages of current Zone and App
		/// </summary>
		public List<ToSicEavDimensions> GetLanguages() => GetDimensionChildren("Culture");


        public string[] GetLanguagesExtNames() => GetLanguages().Select(language => language.ExternalKey).ToArray();


	    // 2017-04-05
        //   /// <summary>
        ///// Get LanguageId
        ///// </summary>
        ///// <param name="externalKey">Usually a Key like en-US or de-DE</param>
        ///// <returns>int or null if not foud</returns>
        //public int? GetLanguageId(string externalKey) 
        //          => GetLanguages().FirstOrDefault(l => l.ExternalKey == externalKey)?.DimensionId;

        //   /// <summary>
        ///// Get ExternalKey for specified LanguageId
        ///// </summary>
        ///// <returns>ExternalKey or null if not found</returns>
        //public string GetLanguageExternalKey(int languageId) 
        //          => GetLanguages().Where(l => l.DimensionId == languageId).Select(l => l.ExternalKey).FirstOrDefault();

        /// <summary>
        /// Add a new Language to current Zone
        /// </summary>
        public ToSicEavDimensions AddLanguage(string name, string externalKey)
		{
			var newLanguage = new ToSicEavDimensions
            {
				Name = name,
				ExternalKey = externalKey,
				Parent = GetDimensionId("Culture", null),
                ZoneId = DbContext.ZoneId
			};
            DbContext.SqlDb.Add(newLanguage);
            DbContext.SqlDb.SaveChanges();
			ClearDimensionsCache();

			return newLanguage;
		}

		///// <summary>
		///// Test whether Zone has any Languages/is multilingual
		///// </summary>
		///// <param name="zoneId">null means ZoneId on current Context</param>
		//public bool HasLanguages(int? zoneId = null) 
  //          => Context.SqlDb.Dimensions.Any(d => d.Parent == Context.SqlDb.Dimensions.FirstOrDefault(p => p.ZoneId == (zoneId ?? Context.ZoneId) && p.ParentID == null && p.SystemKey == Constants.CultureSystemKey));

	    #endregion
	}
}
