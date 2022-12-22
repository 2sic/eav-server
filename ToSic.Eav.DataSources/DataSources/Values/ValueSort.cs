﻿using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
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
        Icon = Icons.Sort,
        Type = DataSourceType.Sort,
        GlobalName = "ToSic.Eav.DataSources.ValueSort, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.ValueSort",
        HelpLink = "https://r.2sxc.org/DsValueSort")]

    public sealed class ValueSort : DataSource
	{

        #region Configuration-properties
        private const string AttrKey = "Attributes";
		private const string DirectionKey = "Directions";
        
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
		public ValueSort(ValueLanguages valLanguages, Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.ValSrt")
        {
            ConnectServices(
                _valLanguages = valLanguages
            );

            Provide(GetValueSort);
		    ConfigMask(AttrKey, $"[Settings:{AttrKey}]");
		    ConfigMask(DirectionKey, $"[Settings:{DirectionKey}]");
		    ConfigMask(ValueLanguages.LangKey, ValueLanguages.LanguageSettingsPlaceholder);
        }
        private readonly ValueLanguages _valLanguages;

        /// <summary>
        /// The internal language list used to lookup values.
        /// It's internal, to allow testing/debugging from the outside
        /// </summary>
        [PrivateApi] internal string[] LanguageList { get; private set; }

        private const char FieldId = 'i';
        private const char FieldMod = 'm';
        private const char FieldTitle = 't';
        private const char FieldCreate = 'c';
        private const char FieldNormal = 'x';

		private IImmutableList<IEntity> GetValueSort()
		{
			var wrapLog = Log.Fn<IImmutableList<IEntity>>();

			// todo: maybe do something about languages?
			// todo: test decimal / number types

			Configuration.Parse();

            Log.A("will apply value-sort");
			var sortAttributes = Attributes.Split(',').Select(s => s.Trim()).ToArray();
			var sortDirections = Directions.Split(',').Select(s => s.Trim()).ToArray();
			var descendingCodes = new[] { "desc","d","0",">" };

			// Languages check - not fully implemented yet, only supports "default" / "current"
            LanguageList = _valLanguages.PrepareLanguageList(Languages, Log);

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");

            // check if no list parameters specified
		    if (sortAttributes.Length == 1 && string.IsNullOrWhiteSpace(sortAttributes[0]))
		        return wrapLog.Return(originals, "no params");

			// 2022-03-09 2dm
			// Previously we had some code which extracted "unsortable" items
			// Based on the fact that they didn't have certain properties which were to be sorted on
			// Now I plan to change it back so it won't optimize this
			// And let LINQ handle null as before/after
			// Plan is to leave this original code and comment in till ca. middle of 2022, in case something breaks
            var results = originals;
            // only keep entities that have the expected attributes (but don't test for id/title, as all have these)
            //var valueAttrs = sortAttributes.Where(v => !Data.Attributes.InternalOnlyIsSpecialEntityProperty(v)).ToArray();
			//var results = valueAttrs.Length == 0
			//	? originals
			//	: originals
			//		.Where(e =>
   //                 {
			//			if(e.Attributes.Keys.Where(valueAttrs.Contains).Count() != valueAttrs.Length) return false;
			//			var allNotNull = valueAttrs.All(va => e.GetBestValue(va, LanguageList) != null);
			//			return allNotNull;
   //                 })
			//		.ToImmutableArray();

			// if list is blank, then it didn't find the attribute to sort by - so just return unsorted
			// note 2020-10-07 this may have been a bug previously, returning an empty list instead
            if (!results.Any()) return wrapLog.Return(originals, "sort-attribute not found in data");

            // Keep entities which cannot sort by the required values (removed previously from results)
            //var unsortable = originals.Where(e => !results.Contains(e)).ToImmutableArray();

            IOrderedEnumerable<IEntity> ordered = null;

			for (var i = 0; i < sortAttributes.Length; i++)
			{
				// get attribute-name and type; set type=id|title for special cases
				var a = sortAttributes[i];
			    var aLow = a.ToLowerInvariant();
				var specAttr = aLow == Data.Attributes.EntityFieldId ? FieldId
                    : aLow == Data.Attributes.EntityFieldTitle ? FieldTitle 
                    : aLow == Data.Attributes.EntityFieldModified ? FieldMod
                    : aLow == Data.Attributes.EntityFieldCreated ? FieldCreate
                    : FieldNormal;
				var isAscending = true;			// default
				if (sortDirections.Length - 1 >= i)	// if this value has a direction specified, use that...
					isAscending = !descendingCodes.Any(sortDirections[i].ToLowerInvariant().Trim().Contains);

                var getValue = GetPropertyToSortFunc(specAttr, a, LanguageList);

                ordered = ordered == null
                    // First sort - no ordered data yet
                    ? isAscending ? results.OrderBy(getValue) : results.OrderByDescending(getValue)
                    // Following sorts, extend previous sort
                    : isAscending ? ordered.ThenBy(getValue) : ordered.ThenByDescending(getValue);
            }

			ImmutableArray<IEntity> final;
			try
			{
				final = ordered?.ToImmutableArray() ?? ImmutableArray<IEntity>.Empty;
			}
			catch (Exception e)
            {
				return SetError("Error sorting", "Sorting failed - see exception in insights", e);
            }

            final = final/*.AddRange(unsortable)*/.ToImmutableArray();
			return wrapLog.ReturnAsOk(final);
		}

        private Func<IEntity, object> GetPropertyToSortFunc(char propertyCode, string fieldName, string[] languages)
        {
            switch (propertyCode)
            {
				case FieldId: return e => e.EntityId;
				case FieldMod: return e => e.Modified;
				case FieldCreate: return e => e.Created;
				case FieldTitle: return e => e.GetBestTitle(languages);
				default: return e => e.GetBestValue(fieldName, languages);
            }
        }

		//private object GetPropertyToSort(IEntity e, string a, char special)
		//{
		//	// get either the special id or title, if title or normal field, then use language [0] = default
  //          return special == FieldId
  //              ? e.EntityId
  //              : special == FieldMod
  //                  ? e.Modified
  //                  : special == FieldCreate
  //                      ? e.Created
  //                      : special == FieldTitle
  //                          ? e.GetBestTitle(LanguageList)
  //                          : e.GetBestValue(a, LanguageList);
  //          // note 2020-11-17 changed it from the line below to the above, to support languages    
  //          //: (special == 't' ? e.Title : e[a])[0];
  //      }
	}
}