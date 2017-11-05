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
	[DataSourceProperties(Type = DataSourceType.Lookup, In = new[] { Constants.DefaultStreamName, Constants.FallbackStreamName })]
    public sealed class RelationshipFilter : BaseDataSource
	{
        #region Configuration-properties
	    public override string LogId => "DS.RelatF";

        private const string RelationshipKey = "Relationship";
		private const string FilterKey = "Filter";
		private const string CompareAttributeKey = "CompareAttribute";
		private const string CompareModeKey = "Mode";
		private const string ChildOrParentKey = "ChildOrParent";
		private readonly string[] _childOrParentPossibleValues = { "child" };//, "parent"};
		private readonly string[] _compareModeValues = { "default", "contains" };
		//private const string ParentTypeKey = "ParentType";
		//private const string PassThroughOnEmptyFilterKey = "PassThroughOnEmptyFilter";


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

		//2dm maybe a feature for the future, not sure
		//public string ParentType
		//{
		//	get { return Configuration[ParentTypeKey]; }
		//	set { Configuration[ParentTypeKey] = value; }
		//}

		/// <summary>
		/// Comparison mode.
		/// "default" and "contains" will check if such a relationship is available
		/// other modes like "equals" or "exclude" not implemented
		/// </summary>
		public string CompareMode
		{
			get => Configuration[CompareModeKey];
		    set
			{
				if (_compareModeValues.Contains(value.ToLower()))
					Configuration[CompareModeKey] = value.ToLower();
				else
					throw new Exception("Value '" + value + "' not allowed for CompareMode");

			}
		}

		public string ChildOrParent
		{
			get => Configuration[ChildOrParentKey];
		    set
			{
				if (_childOrParentPossibleValues.Contains(value.ToLower()))
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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetEntitiesOrFallback));
			Configuration.Add(RelationshipKey, "[Settings:Relationship]");
			Configuration.Add(FilterKey, "[Settings:Filter]");
		    Configuration.Add(CompareAttributeKey, Constants.EntityFieldTitle);
			Configuration.Add(CompareModeKey, "default");
			Configuration.Add(ChildOrParentKey, "child");
			//Configuration.Add(ParentTypeKey, "");
            //Configuration.Add(PassThroughOnEmptyFilterKey, "[Settings:PassThroughOnEmptyFilter||false]");

            CacheRelevantConfigurations = new[] { RelationshipKey, FilterKey, CompareAttributeKey, CompareModeKey, ChildOrParentKey};
        }

        private IEnumerable<IEntity> GetEntitiesOrFallback()
        {
            var res = GetEntities();
            // ReSharper disable PossibleMultipleEnumeration
            if (!res.Any())
                if (In.ContainsKey(Constants.FallbackStreamName) && In[Constants.FallbackStreamName] != null && In[Constants.FallbackStreamName].LightList.Any())
                    res = In[Constants.FallbackStreamName].LightList;

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
			var mode = CompareMode.ToLower();
			if (mode == "contains") mode = "default";
			if (mode != "default")
				throw new Exception("Can't use CompareMode other than 'default'");
			var childParent = ChildOrParent;
			if (!_childOrParentPossibleValues.Contains(childParent)) // != "child")
				throw new Exception("can only find related children at the moment, must set ChildOrParent to 'child'");
			//var lang = Languages.ToLower();
			//if (lang != "default")
			//	throw new Exception("Can't filter for languages other than 'default'");
			//if (lang == "default") lang = ""; // no language is automatically the default language

		    var lowAttribName = compAttr.ToLower();
		    Log.Add($"get related on attr:{compAttr}, filter:{filter}, mode:{mode}, child/parent:{childParent}");
            //var specAttr = lowAttribName == Constants.EntityFieldAutoSelect ? 'a' : lowAttribName == Constants.EntityFieldId ? 'i' : lowAttribName == Constants.EntityFieldTitle ? 't' : 'x';

			var originals = In[Constants.DefaultStreamName].LightList;

            var compType = lowAttribName == Constants.EntityFieldAutoSelect ? CompareType.Auto : lowAttribName == Constants.EntityFieldId ? CompareType.Id : lowAttribName == Constants.EntityFieldTitle ? CompareType.Title : CompareType.Any;

            //if (string.IsNullOrWhiteSpace(_filter) && PassThroughOnEmptyFilter)
            //	return originals;

            // only get those, having a relationship on this name
            var results = // (ChildOrParent == "child") ?
				from e in originals
				where e.Relationships.Children[relationship].Any()
				select e;
			//: (from e in originals
			//	where e.Value.Relationships.AllParents.Any(p => p.Type.Name == ParentType)
			//	select e);

			if (ChildOrParent == "child")
			{
			    if (compType == CompareType.Auto)
			        results = results
			            .Where(e => e.Relationships.Children[relationship]
			                .Any(x => getStringToCompare(x, compAttr, CompareType.Id)?.ToLower() == filter
			                          || getStringToCompare(x, compAttr, CompareType.Title)?.ToLower() == filter));
			    else
			        results = results.Where(e =>
			            e.Relationships.Children[relationship]
			                .Any(x => getStringToCompare(x, compAttr, compType)?.ToLower() == filter));
			}
			else
			{
				throw new NotImplementedException("using 'parent' not supported yet, use 'child' to filter'");
				//results = (from e in results
				//		   where e.Value.Relationships.AllParents.Any(x => getStringToCompare(x, compAttr, specAttr) == _filter)
				//		   select e);
			}

			return results;
		}

		//private string getStringToCompare(IEntity e, string a, char special)
		//{
		//	try
		//	{
		//		// get either the special id or title, if title or normal field, then use language [0] = default
		//	    if (e == null) return null;
		//		return special == 'i' ? e.EntityId.ToString() : (special == 't' ? e.Title : e[a])?[0]?.ToString();
		//	}
		//	catch
		//	{
		//		throw new Exception(
		//		    $"Error while trying to filter for related entities. Probably comparing an attribute on the related entity that doesn\'t exist. Was trying to compare the attribute \'{a}\'");
		//	}
		//}
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
