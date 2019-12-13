using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Sort Entity by values in specified Attributes
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.ValueSort, ToSic.Eav.DataSources",
        Type = DataSourceType.Sort,
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.ValueSort",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ValueSort")]

    public sealed class ValueSort : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.ValueS";

        private const string AttrKey = "Attributes";
		private const string DirectionKey = "Value";
		private const string LangKey = "Language";
        
		/// <summary>
		/// The attribute whose value will be sorted by.
		/// </summary>
		public string Attributes
		{
			get => Configuration[AttrKey];
		    set => Configuration[AttrKey] = value;
		}

		/// <summary>
		/// The sorting direction like 'asc' or 'desc', can also be 0, 1
		/// </summary>
		public string Directions
		{
			get => Configuration[DirectionKey];
		    set => Configuration[DirectionKey] = value;
		}

		/// <summary>
		/// Language to filter for. At the moment it is not used, or it is trying to find "any"
		/// </summary>
		public string Languages
		{
			get => Configuration[LangKey];
		    set => Configuration[LangKey] = value;
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new ValueSort
		/// </summary>
		[PrivateApi]
		public ValueSort()
		{
			Provide(GetList);
		    ConfigMask(AttrKey, "[Settings:Attributes]");
		    ConfigMask(DirectionKey, "[Settings:Directions]");
		    ConfigMask(LangKey, "Default");
        }

		private IEnumerable<IEntity> GetList()
		{
			// todo: maybe do something about languages?
			// todo: test datetime & decimal types

			ConfigurationParse();

		    Log.Add("will apply value-sort");
			var attr = Attributes.Split(',').Select(s => s.Trim()).ToArray();
			var directions = Directions.Split(',').Select(s => s.Trim()).ToArray();
			var descendingCodes = new[] { "desc","d","0",">" };

			#region Languages check - not fully implemented yet, only supports "default"
			var lang = Languages.ToLower();
			if (lang != "default")
				throw new Exception("Can't filter for languages other than 'default'");

			if (lang == "default") lang = ""; // no language is automatically the default language

			if (lang == "any")
				throw new NotImplementedException("language 'any' not implemented yet");
			#endregion

            var list = In[Constants.DefaultStreamName].List;

            // check if no list parameters specified
		    if (attr.Length == 1 && string.IsNullOrWhiteSpace(attr[0]))
		        return list;

            // only get the entities, that have these attributes (but don't test for id/title, as all have these)
            var valueAttrs = attr.Where(v => !Constants.InternalOnlyIsSpecialEntityProperty(v)).ToArray();
		    var results = valueAttrs.Length == 0
		        ? list
		        : (from e in list
		            where e.Attributes.Keys.Where(valueAttrs.Contains).Count() == valueAttrs.Length
		            select e);

			// if list is blank, stop here and return blank list
			if (!results.Any())
				return results;

            IOrderedEnumerable<IEntity> ordered = null;

			for (var i = 0; i < attr.Length; i++)
			{
				// get attribute-name and type; set type=id|title for special cases
				var a = attr[i];
			    var aLow = a.ToLower();
				var specAttr = aLow == Constants.EntityFieldId ? 'i' 
                    : aLow == Constants.EntityFieldTitle ? 't' 
                    : aLow == Constants.EntityFieldModified ? 'm'
                    : 'x';
				var isAscending = true;			// default
				if (directions.Length - 1 >= i)	// if this value has a direction specified, use that...
					isAscending = !descendingCodes.Any(directions[i].ToLower().Trim().Contains);

				if (ordered == null)
				{
					// First sort...
					ordered = isAscending
						? results.OrderBy(e => getObjToSort(e, a, specAttr))
						: results.OrderByDescending(e => getObjToSort(e, a, specAttr));
				}
				else
				{
					// following sorts...
					ordered = isAscending
						? ordered.ThenBy(e => getObjToSort(e, a, specAttr))
						: ordered.ThenByDescending(e => getObjToSort(e, a, specAttr));
				}
			}

			return ordered;
		}

		private object getObjToSort(IEntity e, string a, char special)
		{
			// get either the special id or title, if title or normal field, then use language [0] = default
			return special == 'i' ? e.EntityId 
                : special == 'm' ? e.Modified
                : (special == 't' ? e.Title : e[a])[0];
		}
	}
}