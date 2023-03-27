using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSources.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Return only Entities having a specific value in an Attribute/Property
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "Value Filter",
        UiHint = "Keep items which have a property with the expected value",
        Icon = Icons.FilterList,
        Type = DataSourceType.Filter,
        NameId = "ToSic.Eav.DataSources.ValueFilter, ToSic.Eav.DataSources",
        In = new[] { QueryConstants.InStreamDefaultRequired, StreamFallbackName },
        DynamicOut = false,
        ConfigurationType = "|Config ToSic.Eav.DataSources.ValueFilter",
        HelpLink = "https://r.2sxc.org/DsValueFilter")]

    public sealed class ValueFilter : DataSource
    {
        #region Configuration-properties Attribute, Value, Language, Operator

        /// <summary>
		/// The attribute whose value will be scanned / filtered.
		/// </summary>
        [Configuration]
        public string Attribute
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }

        /// <summary>
        /// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
        /// </summary>
        [Configuration]
        public string Value
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

        /// <summary>
        /// The comparison operator, == by default, many possibilities exist
        /// depending on the original types we're comparing
        /// </summary>
        [Configuration(Fallback = "==")]
        public string Operator
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }

        /// <summary>
        /// Amount of items to take - then stop filtering. For performance optimization.
        /// </summary>
        [Configuration]
        public string Take
        {
            get => Configuration.GetThis();
            set => Configuration.SetThisObsolete(value);
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new ValueFilter
        /// </summary>
        [PrivateApi]
        public ValueFilter(ValueLanguages valLanguages, MyServices services) : base(services, $"{LogPrefix}.ValFil")
        {
            ConnectServices(
                _valueLanguageService = valLanguages
            );

            ProvideOut(GetValueFilterOrFallback);
        }
        private readonly ValueLanguages _valueLanguageService;

        private IImmutableList<IEntity> GetValueFilterOrFallback() => Log.Func(() =>
        {
            // todo: maybe do something about languages?
            Configuration.Parse();

            // Get the data, then see if anything came back
            var res = GetValueFilter();
            return res.Any()
                ? (res, "found")
                : In.HasStreamWithItems(StreamFallbackName)
                    ? (In[StreamFallbackName].List.ToImmutableList(), "fallback")
                    : (res, "final");
        });


        private IImmutableList<IEntity> GetValueFilter() => Log.Func(() =>
        {
            Log.A("applying value filter...");
            var fieldName = Attribute;

            var languages = _valueLanguageService.PrepareLanguageList(Languages);

            // Get the In-list and stop if error or empty
            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");
            if (!source.Any()) return (source, "empty");

            var op = Operator.ToLowerInvariant();

            // Case 1/2: Handle basic "none" and "all" operators
            if (op == CompareOperators.OpNone)
                return (EmptyList, CompareOperators.OpNone);
            if (op == CompareOperators.OpAll)
                return (ApplyTake(source).ToImmutableList(), CompareOperators.OpAll);

            // Case 3: Real filter
            // Find first Entity which has this property being not null to detect type
            var (isSpecial, fieldType) = Attributes.InternalOnlyIsSpecialEntityProperty(fieldName);
            var firstEntity = isSpecial
                ? source.FirstOrDefault()
                : source.FirstOrDefault(x => x.Attributes.ContainsKey(fieldName) && x.Value(fieldName) != null)
                  // 2022-03-09 2dm If none is found with a real value, get the first that has this attribute
                  ?? source.FirstOrDefault(x => x.Attributes.ContainsKey(fieldName));

            // if I can't find any, return empty list
            if (firstEntity == null)
                return (EmptyList, "empty");

            // New mechanism because the filter previously ignored internal properties like Modified, EntityId etc.
            // Using .Value should get everything, incl. modified, EntityId, EntityGuid etc.
            // 2022-03-09 2dm 
            if (!isSpecial) fieldType = firstEntity[fieldName].Type;
            //var firstValue = firstEntity.Value(fieldName);


            // 2021-03-29 2dm changed from checking the type-name to actually checking the type
            // this was necessary, because entity-lists were LazyEntities and not "Entity"
            //var netTypeName = firstAtt?.GetType().Name ?? "Null";
            // very special case - since we're using the .net type and not the Attribute.Type,
            // then lazy-entities are marked as LazyEntity or similar, and NOT "Entity"
            //if (netTypeName.Contains(Constants.DataTypeEntity)) netTypeName = Constants.DataTypeEntity;

            IImmutableList<IEntity> innerErrors = null;
            var compMaker = new ValueComparison((title, message) => innerErrors = Error.Create(title: title, message: message), Log);
            var compare = compMaker.GetComparison(fieldType, fieldName, op, languages, Value);

            return innerErrors.SafeAny()
                ? (innerErrors, "error") 
                : (GetFilteredWithLinq(source, compare), "ok");

            // Note: the alternate GetFilteredWithLoop has more logging, activate in serious cases
            // Note that the code might not be 100% identical, but it should help find issues
        });


        private IImmutableList<IEntity> GetFilteredWithLinq(IEnumerable<IEntity> originals, Func<IEntity, bool> compare) => Log.Func(() =>
        {
            try
            {
                var results = originals.Where(compare);
                results = ApplyTake(results);
                return (results.ToImmutableList(), "ok");
            }
            catch (Exception ex)
            {
                return (Error.Create(title: "Unexpected Error",
                    message: "Experienced error while executing the filter LINQ. " +
                             "Probably something with type-mismatch or the same field using different types or null. " +
                             "The exception was logged to Insights.",
                    exception: ex), "error");
            }
        });

        private IEnumerable<IEntity> ApplyTake(IEnumerable<IEntity> results) => Log.Func(() => int.TryParse(Take, out var tk)
            ? (results.Take(tk), $"take {tk}")
            : (results, "take all"));


        /// <summary>
        /// A helper function to apply the filter without LINQ - ideal when trying to debug exactly what value crashed
        /// </summary>
        /// <param name="inList"></param>
        /// <param name="attr"></param>
        /// <param name="lang"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local
        private IEnumerable<IEntity> GetFilteredWithLoop(IEnumerable<IEntity> inList, string attr, string lang, string filter)
        {
            var result = new List<IEntity>();
            var langArr = new[] { lang };
            foreach (var res in inList)
                //try
                //{
                //if (res.Value[attr][lang].ToString() == filter)
                if ((res.GetBestValue(attr, langArr) ?? "").ToString() == filter)
                    result.Add(res);
            //}
            //catch { }
            return result;
        }

    }
}