using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Attributes;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Filter Entities by Value in a Related Entity
	/// </summary>
	[PipelineDesigner]
	[DataSourceProperties(
        Type = DataSourceType.Lookup, 
        In = new[] { Constants.DefaultStreamName, Constants.FallbackStreamName }, 
        DynamicOut = false,
	    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-RelationshipFilter")]
    public sealed class RelationshipFilter : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.RelatF";

	    public const string SettingsRelationship = "Relationship";
	    public const string SettingsFilter = "Filter";
	    public const string SettingsRelAttribute = "AttributeOnRelationship"; // todo
	    public const string SettingsCompareMode = "Comparison"; 
	    public const string SettingsDirection = "Direction"; // todo
	    public const string SettingsSeparator = "Separator";

	    private const string PrefixNot = "not-";
        private const string RelationshipKey = "Relationship";
		private const string FilterKey = "Filter";
		private const string CompareAttributeKey = "CompareAttribute";
		private const string CompareModeKey = "Mode";
	    private const string SeparatorKey = "Separator";
		private const string ChildOrParentKey = "ChildOrParent";
	    private const string DefaultDirection = "child";
	    private const string DefaultSeparator = "ignore"; // by default, don't separate!
		private readonly string[] _directionPossibleValues = { DefaultDirection, "parent"};
		//private readonly string[] _compareModeValues = { "default", "contains" };
		//private const string ParentTypeKey = "ParentType";
		//private const string PassThroughOnEmptyFilterKey = "PassThroughOnEmptyFilter";


            public enum CompareModes
            {
                contains,
                containsall,
                todocontainsany,
                todocontainsnone,
                first,
            }

	    private enum CompareType { Any, Id, Title, Auto }

		//private const string LangKey = "Language";
		/// <summary>
		/// The attribute whoose value will be filtered
		/// </summary>
		public string Relationship
		{
			get => Configuration[RelationshipKey];
		    set => Configuration[RelationshipKey] = value;
		}

		/// <summary>
		/// The filter that will be used - for example "Daniel" when looking for an entity w/the value Daniel
		/// </summary>
		public string Filter
		{
			get => Configuration[FilterKey];
		    set => Configuration[FilterKey] = value;
		}

		public string CompareAttribute
		{
			get => Configuration[CompareAttributeKey];
		    set => Configuration[CompareAttributeKey] = value;
		}

		/// <summary>
		/// Comparison mode.
		/// "default" and "contains" will check if such a relationship is available
		/// other modes like "equals" or "exclude" not implemented
		/// </summary>
		public string CompareMode
		{
			get => Configuration[CompareModeKey];
		    set => Configuration[CompareModeKey] = value.ToLower();
		}
		public string Separator
		{
			get => Configuration[SeparatorKey];
		    set => Configuration[SeparatorKey] = value.ToLower();
		}

		public string ChildOrParent
		{
			get => Configuration[ChildOrParentKey];
		    set
			{
				if (_directionPossibleValues.Contains(value.ToLower()))
					Configuration[ChildOrParentKey] = value.ToLower();
				else
					throw new Exception("Value '" + value + "'not allowed for ChildOrParent");
			}
		}

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Language to filter for. At the moment it is not used, or it is trying to find "any"
        /// </summary>
        /// <summary>
        /// Constructs a new RelationshipFilter
        /// </summary>
        public RelationshipFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetEntitiesOrFallback));
			Configuration.Add(RelationshipKey, $"[Settings:{SettingsRelationship}]");
			Configuration.Add(FilterKey, $"[Settings:{SettingsFilter}]");
		    Configuration.Add(CompareAttributeKey, $"[Settings:{SettingsRelAttribute}||{Constants.EntityFieldTitle}]");
			Configuration.Add(CompareModeKey, $"[Settings:{SettingsCompareMode}||{CompareModes.contains}]");
			Configuration.Add(SeparatorKey, $"[Settings:{SettingsSeparator}||{DefaultSeparator}]");
			Configuration.Add(ChildOrParentKey, $"[Settings:{SettingsDirection}||{DefaultDirection}]");

            CacheRelevantConfigurations = new[] { RelationshipKey, FilterKey, CompareAttributeKey, CompareModeKey, ChildOrParentKey};
        }

        private IEnumerable<IEntity> GetEntitiesOrFallback()
        {
            var res = GetEntities();
            // ReSharper disable PossibleMultipleEnumeration
            if (!res.Any())
                if (In.ContainsKey(Constants.FallbackStreamName) && In[Constants.FallbackStreamName] != null && In[Constants.FallbackStreamName].List.Any())
                    res = In[Constants.FallbackStreamName].List;

            return res;
            // ReSharper restore PossibleMultipleEnumeration
        }

        private IEnumerable<IEntity> GetEntities()
		{
			// todo: maybe do something about languages?

			EnsureConfigurationIsLoaded();

			var relationship = Relationship;
			var compAttr = CompareAttribute;
			var filter = Filter.ToLower(); // new: make case insensitive
			var strMode = CompareMode.ToLower();
		    var useNot = strMode.StartsWith(PrefixNot);
		    if (useNot) strMode = strMode.Substring(PrefixNot.Length);

			if (strMode == "default") strMode = "contains"; // 2017-11-18 old default was "default" - this is still in for compatibility
            if(!Enum.TryParse<CompareModes>(strMode, true, out var mode))
				throw new Exception("Can't use CompareMode other than 'default'");

			var childParent = ChildOrParent;
			if (!_directionPossibleValues.Contains(childParent)) // != "child")
				throw new Exception("can only find related children at the moment, must set ChildOrParent to 'child'");
			//var lang = Languages.ToLower();
			//if (lang != "default")
			//	throw new Exception("Can't filter for languages other than 'default'");
			//if (lang == "default") lang = ""; // no language is automatically the default language

		    var lowAttribName = compAttr.ToLower();
		    Log.Add($"get related on relationship:'{relationship}', filter:'{filter}', rel-field:'{compAttr}' mode:'{mode}', child/parent:'{childParent}'");

			var originals = In[Constants.DefaultStreamName].List;

		    var compType = lowAttribName == Constants.EntityFieldAutoSelect
		        ? CompareType.Auto
		        : lowAttribName == Constants.EntityFieldId
		            ? CompareType.Id
		            : lowAttribName == Constants.EntityFieldTitle
		                ? CompareType.Title
		                : CompareType.Any;
            //if (string.IsNullOrWhiteSpace(_filter) && PassThroughOnEmptyFilter)
            //	return originals;

            // only get those, having a relationship on this name
		    var query = originals;
            // by default, skip all which don't have anything, but not if we're finding the "not" list
		    if (!useNot) query = query.Where(e => e.Relationships.Children[relationship].Any());

            // pick the correct value-comparison
		    var comparisonOnRelatedItem = compType == CompareType.Auto
			    ? CompareTitleOrId(compAttr)
			    : CompareField(compAttr, compType);


		    var filterList = Separator == DefaultSeparator
		        ? new[] {filter}
		        : filter.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);


		    Log.Add($"will compare on:{compType} '{lowAttribName}', values to check ({filterList.Length}):'{filter}'");

            // pick the correct list-comparison - atm 2 options
		    var modeCompare = mode == CompareModes.containsall
		        ? ModeContainsAll(relationship, filterList, comparisonOnRelatedItem)
		        : ModeContainsOne(relationship, filter, comparisonOnRelatedItem);

		    if (useNot) modeCompare = ModeNot(modeCompare);

		    if (ChildOrParent == "child")
		        query = query.Where(modeCompare);
		    else
		    {
		        throw new NotImplementedException("using 'parent' not supported yet, use 'child' to filter'");
		        //results = (from e in results
		        //		   where e.Value.Relationships.AllParents.Any(x => getStringToCompare(x, compAttr, specAttr) == _filter)
		        //		   select e);
		    }
		    var results = query.ToList();

		    Log.Add($"found in relationship-filter {results.Count}");
			return results;
		}

	    private static Func<IEntity, bool> ModeNot(Func<IEntity, bool> innerFunc) => e => !innerFunc(e);

	    private static Func<IEntity, bool> ModeContainsAll(string relationship, string[] filterList,
	        Func<IEntity, string, bool> internalCompare)
	        => entity =>
	        {
	            var rels = entity.Relationships.Children[relationship];
	            return filterList.All(v => rels.Any(r => internalCompare(r, v)));
	        };

	    private static Func<IEntity, bool> ModeContainsSome(string relationship, string[] filterList,
	        Func<IEntity, string, bool> internalCompare)
	        => entity =>
	        {
	            var rels = entity.Relationships.Children[relationship];
	            return filterList.Any(v => rels.Any(r => internalCompare(r, v)));
	        };

	    private static Func<IEntity, bool> ModeContainsOne(string relationship, string value,
	        Func<IEntity, string, bool> internalCompare)
	        => entity => entity.Relationships.Children[relationship]
	            .Any(r => internalCompare(r, value));

	    private Func<IEntity, string, bool> CompareTitleOrId(string compAttr)
	        => (entity, filter) => getStringToCompare(entity, compAttr, CompareType.Id)?.ToLower() == filter
	                               || getStringToCompare(entity, compAttr, CompareType.Title)?.ToLower() == filter;

	    private Func<IEntity, string, bool> CompareField(string compAttr, CompareType compType) 
            => (entity, value) => getStringToCompare(entity, compAttr, compType)?.ToLower() == value;


        private string getStringToCompare(IEntity e, string a, CompareType special)
		{
			try
			{
				// get either the special id or title, if title or normal field, then use language [0] = default
			    if (e == null) return null;
				return special == CompareType.Id ? e.EntityId.ToString() : (special == CompareType.Title ? e.Title : e[a])?[0]?.ToString();
		}
			catch
			{
				throw new Exception(
				    $"Error while trying to filter for related entities. Probably comparing an attribute on the related entity that doesn\'t exist. Was trying to compare the attribute \'{a}\'");
			}
		}
	}
}
