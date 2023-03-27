using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;
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
        NameId = "ToSic.Eav.DataSources.ValueSort, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { QueryConstants.InStreamDefaultRequired },
	    ConfigurationType = "|Config ToSic.Eav.DataSources.ValueSort",
        HelpLink = "https://r.2sxc.org/DsValueSort")]

    public sealed class ValueSort : DataSource
	{
        #region Configuration-properties
        
		/// <summary>
		/// The attribute whose value will be sorted by.
		/// </summary>
		[Configuration]
		public string Attributes
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }

        /// <summary>
        /// The sorting direction like 'asc' or 'desc', can also be 0, 1
        /// </summary>
        [Configuration]
		public string Directions
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }

        /// <summary>
        /// Language to filter for. At the moment it is not used, or it is trying to find "any"
        /// </summary>
        [Configuration(Fallback = ValueLanguages.LanguageDefaultPlaceholder)]
		public string Languages
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }
		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new ValueSort
		/// </summary>
		[PrivateApi]
		public ValueSort(ValueLanguages valLanguages, MyServices services) : base(services, $"{LogPrefix}.ValSrt")
        {
            ConnectServices(
                _valLanguages = valLanguages
            );

            ProvideOut(GetValueSort);
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

		private IImmutableList<IEntity> GetValueSort() => Log.Func(l =>
        {
			// todo: maybe do something about languages?
			// todo: test decimal / number types

			Configuration.Parse();

            l.A("will apply value-sort");
			var sortAttributes = Attributes.Split(',').Select(s => s.Trim()).ToArray();
			var sortDirections = Directions.Split(',').Select(s => s.Trim()).ToArray();
			var descendingCodes = new[] { "desc","d","0",">" };

			// Languages check - not fully implemented yet, only supports "default" / "current"
            LanguageList = _valLanguages.PrepareLanguageList(Languages);

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            // check if no list parameters specified
            if (sortAttributes.Length == 1 && string.IsNullOrWhiteSpace(sortAttributes[0]))
		        return (source, "no params");

            // if list is blank, then it didn't find the attribute to sort by - so just return unsorted
			// note 2020-10-07 this may have been a bug previously, returning an empty list instead
            if (!source.Any()) return (source, "sort-attribute not found in data");

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
                    ? isAscending ? source.OrderBy(getValue) : source.OrderByDescending(getValue)
                    // Following sorts, extend previous sort
                    : isAscending ? ordered.ThenBy(getValue) : ordered.ThenByDescending(getValue);
            }

			IImmutableList<IEntity> final;
			try
            {
                final = ordered?.ToImmutableList() ?? EmptyList;
            }
			catch (Exception e)
            {
				return (Error.Create(title: "Error sorting", message: "Sorting failed - see exception in insights", exception: e), "error");
            }

            return (final, "ok");
		});

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
    }
}