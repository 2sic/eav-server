using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.ContentTypes;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        EnableConfig = true,
        ExpectsDataOfType = "5461d34d-7dc6-4d38-9250-a0729cc8ead3",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes")]

    public sealed class Attributes: BaseDataSource
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
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
            Configuration.Add(ContentTypeKey, $"[Settings:{ContentTypeField}||{TryToUseInStream}]");

            CacheRelevantConfigurations = new[] {ContentTypeKey};
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

            IEnumerable<Dictionary<string, object>> list;
            // try to load from type, if it exists
	        list = type?.Attributes?.OrderBy(at => at.Name).Select(at => new Dictionary<string, object>
	        {
	            {AttributeType.Name.ToString(), at.Name},
	            {AttributeType.Type.ToString(), at.Type},
	            {AttributeType.IsTitle.ToString(), at.IsTitle},
	            {AttributeType.SortOrder.ToString(), at.SortOrder}
	        });

            // if it didn't work yet, maybe try from stream items
            if (list == null)
	        {
	            // try first item
	            if (TryToUseInStream != ContentTypeName) return new List<IEntity>();

	            var item = optionalList?.FirstOrDefault();
	            if (item == null) return new List<IEntity>();

	            list = item.Attributes.OrderBy(at => at.Key).Select(at => new Dictionary<string, object>
	            {
	                {AttributeType.Name.ToString(), at.Key},
	                {AttributeType.Type.ToString(), at.Value.Type},
	                {AttributeType.IsTitle.ToString(), false},
	                {AttributeType.SortOrder.ToString(), 0}
	            });
            }

	        //list = attribs.OrderBy(at => at.Name).Select(at => new Dictionary<string, object>
	        //{
	        //    {AttributeType.Name.ToString(), at.Name},
	        //    {AttributeType.Type.ToString(), at.Type},
	        //    {AttributeType.IsTitle.ToString(), at.IsTitle},
	        //    {AttributeType.SortOrder.ToString(), at.SortOrder}
	        //});

            return list.Select(attribData => new Data.Entity(AppId, 0, AttribContentTypeName, attribData, AttributeType.Name.ToString()));
        }



	    //private static AttribInfo BuildAttribInfo(KeyValuePair<string, IAttribute> a) => new AttribInfo
	    //{
	    //    Name = a.Key,
	    //    Type = a.Value.Type,
	    //    SortOrder = 0,
	    //    IsTitle = false
	    //};

	    //private static AttribInfo BuildAttribInfo(IAttributeDefinition at) => new AttribInfo
	    //{
	    //    Name = at.Name,
	    //    Type = at.Type,
	    //    IsTitle = at.IsTitle,
	    //    SortOrder = at.SortOrder
	    //};

	    internal class AttribInfo
	    {
	        public string Name;
            public string Type;

	        public int SortOrder;
	        public bool IsTitle;
	    }

	}
}