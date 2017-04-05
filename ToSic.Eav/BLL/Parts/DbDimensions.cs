using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
	public class DbDimensions: BllCommandBase
	{
        public DbDimensions(DbDataController ctx) : base(ctx) { }
        private static List<Dimension> _cachedDimensions;

        #region Cached Dimensions



	    internal void EnsureDimensionsCache()
        {
            if (_cachedDimensions == null)
                _cachedDimensions = DbContext.SqlDb.Dimensions.ToList();
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

            return _cachedDimensions.Where(d => string.Equals(d.SystemKey, systemKey, StringComparison.InvariantCultureIgnoreCase) && string.Equals(d.ExternalKey, externalKey, StringComparison.InvariantCultureIgnoreCase) && d.ZoneID == DbContext.ZoneId).Select(d => d.DimensionID).FirstOrDefault();
		}

		/// <summary>
		/// Get a single Dimension
		/// </summary>
		/// <returns>A Dimension or null</returns>
		public Dimension GetDimension(int dimensionId) 
            => DbContext.SqlDb.Dimensions.SingleOrDefault(d => d.DimensionID == dimensionId);

	    /// <summary>
		/// Get Dimensions by Ids
		/// </summary>
		internal IEnumerable<Dimension> GetDimensions(IEnumerable<int> dimensionIds) 
            => DbContext.SqlDb.Dimensions.Where(d => dimensionIds.Contains(d.DimensionID) && d.ZoneID == DbContext.ZoneId);

	    /// <summary>
		/// Get a List of Dimensions having specified SystemKey and current ZoneId and AppId
		/// </summary>
		public List<Dimension> GetDimensionChildren(string systemKey)
		{
			EnsureDimensionsCache();

			return _cachedDimensions.Where(d => d.ParentID.HasValue && d.Parent.SystemKey == systemKey && d.ZoneID == DbContext.ZoneId).ToList();
		}

		///// <summary>
		///// Test whehter Value exists on specified Entity and Attribute with specified DimensionIds 
		///// </summary>
		//public bool ValueExists(List<int> dimensionIds, int entityId, int attributeId) 
  //          => Context.SqlDb.Values.Any(v => !v.ChangeLogIDDeleted.HasValue && v.EntityID == entityId && v.AttributeID == attributeId && v.ValuesDimensions.All(d => dimensionIds.Contains(d.DimensionID)));

        // 2017-04-05 2dm clean-up
	 //   /// <summary>
		///// Get Dimensions to use for specified Entity and Attribute if unknown
		///// </summary>
		//public IQueryable<Dimension> GetFallbackDimensions(int entityId, int attributeId)
		//{
  //          return from vd in Context.SqlDb.ValuesDimensions
		//		   where
		//			  vd.Value.EntityID == entityId &&
		//			  vd.Value.AttributeID == attributeId &&
		//			  !vd.Value.ChangeLogIDDeleted.HasValue
		//		   orderby vd.Value.ChangeLogIDCreated
		//		   select
		//			  vd.Dimension;
		//}

		/// <summary>
		/// Update a single Dimension
		/// </summary>
		public void UpdateDimension(int dimensionId, bool? active = null, string name = null)
		{
			var dimension = DbContext.SqlDb.Dimensions.Single(d => d.DimensionID == dimensionId);
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
                UpdateDimension(eavLanguage.DimensionID, active);
            else
                AddLanguage(cultureText, cultureCode);

        }




        /// <summary>
        /// Add a new Dimension
        /// </summary>
        internal void AddDimension(string systemKey, string name, Zone zone, Dimension parent = null, bool autoSave = false)
		{
			var newDimension = new Dimension
			{
				SystemKey = systemKey,
				Name = name,
				Zone = zone,
				Parent = parent
			};
            DbContext.SqlDb.AddToDimensions(newDimension);

			if (autoSave)
                DbContext.SqlDb.SaveChanges();

			ClearDimensionsCache();
		}

		#region Languages

		/// <summary>
		/// Get all Languages of current Zone and App
		/// </summary>
		public List<Dimension> GetLanguages() => GetDimensionChildren("Culture");

        // 2017-04-05
	 //   /// <summary>
		///// Get LanguageId
		///// </summary>
		///// <param name="externalKey">Usually a Key like en-US or de-DE</param>
		///// <returns>int or null if not foud</returns>
		//public int? GetLanguageId(string externalKey) 
  //          => GetLanguages().FirstOrDefault(l => l.ExternalKey == externalKey)?.DimensionID;

	 //   /// <summary>
		///// Get ExternalKey for specified LanguageId
		///// </summary>
		///// <returns>ExternalKey or null if not found</returns>
		//public string GetLanguageExternalKey(int languageId) 
  //          => GetLanguages().Where(l => l.DimensionID == languageId).Select(l => l.ExternalKey).FirstOrDefault();

	    /// <summary>
		/// Add a new Language to current Zone
		/// </summary>
		public Dimension AddLanguage(string name, string externalKey)
		{
			var newLanguage = new Dimension
			{
				Name = name,
				ExternalKey = externalKey,
				ParentID = GetDimensionId("Culture", null),
                ZoneID = DbContext.ZoneId
			};
            DbContext.SqlDb.AddToDimensions(newLanguage);
            DbContext.SqlDb.SaveChanges();
			ClearDimensionsCache();

			return newLanguage;
		}

		///// <summary>
		///// Test whether Zone has any Languages/is multilingual
		///// </summary>
		///// <param name="zoneId">null means ZoneId on current Context</param>
		//public bool HasLanguages(int? zoneId = null) 
  //          => Context.SqlDb.Dimensions.Any(d => d.Parent == Context.SqlDb.Dimensions.FirstOrDefault(p => p.ZoneID == (zoneId ?? Context.ZoneId) && p.ParentID == null && p.SystemKey == Constants.CultureSystemKey));

	    #endregion
	}
}
