using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Caches;
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
	    private const string DefContentType = "not-configured"; // can't be blank, otherwise tokens fail
	    private const string AttribContentTypeName = "EAV_Attribute";
	    
        // 2dm: this is for a later feature...
        private const string AttribCtGuid = "11001010-251c-eafe-2792-000000000004";


        private enum AttributeType
	    {
	        Name,
	        Type,
	        IsTitle,
	        SortOrder
	    }
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
            Configuration.Add(ContentTypeKey, $"[Settings:{ContentTypeField}||{DefContentType}]");

            CacheRelevantConfigurations = new[] {ContentTypeKey};
		}

	    private IEnumerable<IEntity> GetList()
	    {
            EnsureConfigurationIsLoaded();

            // try to load the content-type - if it fails, return empty list
	        if (string.IsNullOrWhiteSpace(ContentTypeName)) return new List<IEntity>();
	        var cache = (BaseCache)DataSource.GetCache(ZoneId, AppId);
	        var type = cache.GetContentType(ContentTypeName);
	        if (type == null) return new List<IEntity>();

	        var list = type.Attributes.OrderBy(at => at.Name).Select(at =>
	        {
	            // Assemble the entities
	            var attribData = new Dictionary<string, object>
	            {
	                {AttributeType.Name.ToString(), at.Name},
	                {AttributeType.Type.ToString(), at.Type},
	                {AttributeType.IsTitle.ToString(), at.IsTitle},
	                {AttributeType.SortOrder.ToString(), at.SortOrder}
	            };

	            return new Data.Entity(AppId, 0, AttribContentTypeName, attribData, AttributeType.Name.ToString());
	        });

            return list;
        }

	}
}