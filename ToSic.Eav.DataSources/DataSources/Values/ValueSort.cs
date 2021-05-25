using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Sort Entity by values in specified Attributes / Properties
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Value Sort",
        UiHint = "Sort items by a property",
        Icon = "sort",
        Type = DataSourceType.Sort,
        GlobalName = "ToSic.Eav.DataSources.ValueSort, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.ValueSort",
        HelpLink = "https://r.2sxc.org/DsValueSort")]

    public sealed class ValueSort : DataSourceBase
	{

        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.ValueS";

        private const string AttrKey = "Attributes";
		private const string DirectionKey = "Value";
		//private const string LangKey = "Language";
        
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
			get => Configuration[ValueLanguages.LangKey];
		    set => Configuration[ValueLanguages.LangKey] = value;
		}
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new ValueSort
		/// </summary>
		[PrivateApi]
		public ValueSort(ValueLanguages valLanguages)
		{
            Provide(GetValueSort);
		    ConfigMask(AttrKey, "[Settings:Attributes]");
		    ConfigMask(DirectionKey, "[Settings:Directions]");
		    ConfigMask(ValueLanguages.LangKey, ValueLanguages.LanguageSettingsPlaceholder);

            _valLanguages = valLanguages.Init(Log);
        }
        private readonly ValueLanguages _valLanguages;

        /// <summary>
        /// The internal language list used to lookup values.
        /// It's internal, to allow testing/debugging from the outside
        /// </summary>
        [PrivateApi] internal string[] LanguageList { get; private set; }


		private IImmutableList<IEntity> GetValueSort()
		{
			var wrapLog = Log.Call<IImmutableList<IEntity>>();

			// todo: maybe do something about languages?
			// todo: test decimal / number types

			Configuration.Parse();

            Log.Add("will apply value-sort");
			var attr = Attributes.Split(',').Select(s => s.Trim()).ToArray();
			var directions = Directions.Split(',').Select(s => s.Trim()).ToArray();
			var descendingCodes = new[] { "desc","d","0",">" };

			// Languages check - not fully implemented yet, only supports "default" / "current"
            LanguageList = _valLanguages.PrepareLanguageList(Languages, Log);

            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            // check if no list parameters specified
		    if (attr.Length == 1 && string.IsNullOrWhiteSpace(attr[0]))
		        return wrapLog("no params", originals);

            // only get the entities, that have these attributes (but don't test for id/title, as all have these)
            var valueAttrs = attr.Where(v => !Data.Attributes.InternalOnlyIsSpecialEntityProperty(v)).ToArray();
		    var results = valueAttrs.Length == 0
		        ? originals
		        : originals.Where(e => e.Attributes.Keys.Where(valueAttrs.Contains).Count() == valueAttrs.Length).ToImmutableArray();

			// if list is blank, then it didn't find the attribute to sort by - so just return unsorted
			// note 2020-10-07 this may have been a bug previously, returning an empty list instead
            if (!results.Any()) return wrapLog("sort-attribute not found", originals);

            var unsortable = originals.Where(e => !results.Contains(e)).ToImmutableArray();

            IOrderedEnumerable<IEntity> ordered = null;

			for (var i = 0; i < attr.Length; i++)
			{
				// get attribute-name and type; set type=id|title for special cases
				var a = attr[i];
			    var aLow = a.ToLowerInvariant();
				var specAttr = aLow == Data.Attributes.EntityFieldId ? 'i' 
                    : aLow == Data.Attributes.EntityFieldTitle ? 't' 
                    : aLow == Data.Attributes.EntityFieldModified ? 'm'
                    : 'x';
				var isAscending = true;			// default
				if (directions.Length - 1 >= i)	// if this value has a direction specified, use that...
					isAscending = !descendingCodes.Any(directions[i].ToLowerInvariant().Trim().Contains);

				if (ordered == null)
				{
					// First sort...
					ordered = isAscending
						? results.OrderBy(e => GetPropertyToSort(e, a, specAttr))
						: results.OrderByDescending(e => GetPropertyToSort(e, a, specAttr));
				}
				else
				{
					// following sorts...
					ordered = isAscending
						? ordered.ThenBy(e => GetPropertyToSort(e, a, specAttr))
						: ordered.ThenByDescending(e => GetPropertyToSort(e, a, specAttr));
				}
			}

            var final = ordered?.ToImmutableArray() ?? ImmutableArray<IEntity>.Empty;
            // final = final.AddRange(unsortable);
			return wrapLog("ok", final.AddRange(unsortable).ToImmutableArray());
		}

		private object GetPropertyToSort(IEntity e, string a, char special)
		{
			// get either the special id or title, if title or normal field, then use language [0] = default
			return special == 'i' ? e.EntityId 
                : special == 'm' ? e.Modified
                    : special == 't' 
                    ? e.GetBestTitle(LanguageList) 
                    : e.GetBestValue(a, LanguageList);
            // note 2020-11-17 changed it from the line below to the above, to support languages    
            //: (special == 't' ? e.Title : e[a])[0];
		}
	}
}