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

		private int GetDimensionId(string systemKey, string externalKey)
		{
		    return DbContext.SqlDb.ToSicEavDimensions.Where(d =>
		            string.Equals(d.Key, systemKey, StringComparison.InvariantCultureIgnoreCase)
		            && d.Matches(externalKey)
		            && d.ZoneId == DbContext.ZoneId)
		        .Select(d => d.DimensionId)
                .FirstOrDefault();
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
		}

        /// <summary>
        /// Add or update a language. Must use this kind of logic because the client doesn't know if a language
        /// is missing, or has been disabled
        /// </summary>
        /// <param name="cultureCode"></param>
        /// <param name="cultureText"></param>
        /// <param name="active"></param>
        internal void AddOrUpdateLanguage(string cultureCode, string cultureText, bool active)
        {
            var eavLanguage = GetLanguages(true).FirstOrDefault(l => l.Matches(cultureCode));
            // If the language exists in EAV, set the active state, else add it
            if (eavLanguage != null)
                UpdateDimension(eavLanguage.DimensionId, active);
            else
                AddLanguage(cultureText, cultureCode);
        }


        /// <summary>
        /// Add a new Dimension at the top of the dimension tree.
        /// This is used by the "create new zone" code
        /// </summary>
        internal void AddRootCultureNode(string systemKey, string name, ToSicEavZones zone)
        {
			var newDimension = new ToSicEavDimensions
            {
				Key = systemKey,
				Name = name,
				Zone = zone,
				ParentNavigation = null
            };
            DbContext.SqlDb.Add(newDimension);
            DbContext.SqlDb.SaveChanges();
		}

		#region Languages

		/// <summary>
		/// Get all Languages of current Zone and App
		/// </summary>
		private List<DimensionDefinition> GetLanguages(bool includeInactive = false)
        {
            return DbContext.SqlDb.ToSicEavDimensions.ToList().Where(d =>
                d.Parent.HasValue
                && d.ParentNavigation.Key == Constants.CultureSystemKey
                && d.ZoneId == DbContext.ZoneId
                && (includeInactive || d.Active)
                ).Cast<DimensionDefinition>().ToList();
        }


     //   /// <summary>
     //   /// Generate a language list which will have at least 1 language in it for import-purposes
     //   /// note that I'm not sure why we are doing this!
     //   /// </summary>
     //   /// <param name="defaultLanguage"></param>
     //   /// <returns></returns>
	    //public List<DimensionDefinition> GetLanguageListForImport(string defaultLanguage)
	    //{
     //       var langs = GetLanguages();
     //       if (langs.Count == 0)
     //           langs.Add(new DimensionDefinition
     //           {
     //               Active = true,
     //               EnvironmentKey = defaultLanguage,
     //               Name = "(added by import System, default language " + defaultLanguage + ")",
     //               Key = Constants.CultureSystemKey
     //           });
	    //    return langs.ToList();
	    //}

        /// <summary>
        /// Add a new Language to current Zone
        /// </summary>
        private void AddLanguage(string name, string externalKey)
		{
			var newLanguage = new ToSicEavDimensions
            {
				Name = name,
				EnvironmentKey = externalKey,
				Parent = GetDimensionId(Constants.CultureSystemKey, null),
                ZoneId = DbContext.ZoneId,
                Active = true
			};
            DbContext.SqlDb.Add(newLanguage);
            DbContext.SqlDb.SaveChanges();
		}
        
	    #endregion
	}
}
