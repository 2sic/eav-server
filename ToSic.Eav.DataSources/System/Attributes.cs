using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Query;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [Query.VisualQuery(GlobalName = "ToSic.Eav.DataSources.System.Attributes, ToSic.Eav.DataSources",
        Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "5461d34d-7dc6-4d38-9250-a0729cc8ead3",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes")]

    public sealed class Attributes: DataSourceBase
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavAts";

        private const string ContentTypeKey = "ContentType";
        private const string ContentTypeField = "ContentTypeName";
	    private const string TryToUseInStream = "not-configured-try-in"; // can't be blank, otherwise tokens fail
	    private const string AttribContentTypeName = "EAV_Attribute";
	    
        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string AttribCtGuid = "11001010-251c-eafe-2792-000000000004";


        /// <summary>
        /// The content-type name
        /// </summary>
        public string ContentTypeName
        {
            get => Configuration[ContentTypeKey];
            set => Configuration[ContentTypeKey] = value;
        }
        
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public Attributes()
		{
			Provide(GetList);
		    ConfigMask(ContentTypeKey, $"[Settings:{ContentTypeField}||{TryToUseInStream}]");
		}

	    private IEnumerable<IEntity> GetList()
	    {
            EnsureConfigurationIsLoaded();

	        IContentType type;
            // try to load the content-type - if it fails, return empty list
	        if (string.IsNullOrWhiteSpace(ContentTypeName)) return new List<IEntity>();

	        var useStream = TryToUseInStream == ContentTypeName && In.ContainsKey(Constants.DefaultStreamName);
	        var optionalList = useStream
	            ? In[Constants.DefaultStreamName]?.List?.ToList()
	            : null;

	        type = useStream 
                ? optionalList?.FirstOrDefault()?.Type 
                : DataSource.GetCache(ZoneId, AppId).GetContentType(ContentTypeName);

	        // try to load from type, if it exists
	        var list = type?.Attributes?.OrderBy(at => at.Name).Select(BuildDictionary).ToList();

            // note that often dynamic types will have zero attributes, so the previous command
            // gives a 0-list of attributes
            // so if that's the case, check the first item in the results
	        if (list == null || list.Count == 0)
	            list = TryToUseInStream == ContentTypeName
	                ? optionalList?.FirstOrDefault()?.Attributes
	                    .OrderBy(at => at.Key)
	                    .Select(BuildDictionary)
	                    .ToList()
	                : null;

            // if it didn't work yet, maybe try from stream items

	        return list?.Select(attribData => AsEntity(attribData, AttributeType.Name.ToString(), AttribContentTypeName) // new Data.Entity(AppId, 0, AttribContentTypeName, attribData, AttributeType.Name.ToString())
            ) 
                ?? new List<IEntity>();
        }



	    private static Dictionary<string, object> BuildDictionary(KeyValuePair<string, IAttribute> at) => new Dictionary<string, object>
	    {
	        {AttributeType.Name.ToString(), at.Key},
	        {AttributeType.Type.ToString(), at.Value.Type},
	        {AttributeType.IsTitle.ToString(), false},
	        {AttributeType.SortOrder.ToString(), 0}
	    };

	    private static Dictionary<string, object> BuildDictionary(IContentTypeAttribute at) => new Dictionary<string, object>
	    {
	        {AttributeType.Name.ToString(), at.Name},
	        {AttributeType.Type.ToString(), at.Type},
	        {AttributeType.IsTitle.ToString(), at.IsTitle},
	        {AttributeType.SortOrder.ToString(), at.SortOrder}
	    };
	}
}