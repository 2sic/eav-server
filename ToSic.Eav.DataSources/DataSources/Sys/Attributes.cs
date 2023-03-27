using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.Documentation;
using static ToSic.Eav.DataSources.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    /// <remarks>
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Attributes of Type",
        UiHint = "Attributes/fields of a Content-Type",
        Icon = Icons.Dns,
        Type = DataSourceType.System,
        NameId = "ToSic.Eav.DataSources.System.Attributes, ToSic.Eav.DataSources",
        Audience = Audience.Advanced,
        DynamicOut = false,
        ConfigurationType = "5461d34d-7dc6-4d38-9250-a0729cc8ead3",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes")]

    public sealed class Attributes: DataSource
	{

        #region Configuration-properties (no config)

	    private const string TryToUseInStream = "not-configured-try-in"; // can't be blank, otherwise tokens fail
	    private const string AttribContentTypeName = "Attribute";
	    
        /// <summary>
        /// The content-type name
        /// </summary>
        [Configuration(Fallback = TryToUseInStream)]
        public string ContentTypeName => Configuration.GetThis();
        
		#endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
		public Attributes(IAppStates appStates, MyServices services, IDataFactory dataFactory) : base(services, $"{LogPrefix}.Attrib")
        {
            ConnectServices(
                _appStates = appStates,
                _dataFactory = dataFactory.New(options: new DataFactoryOptions(typeName: AttribContentTypeName, titleField: AttributeType.Title.ToString()))
            );
            ProvideOut(GetList);
		}
        private readonly IAppStates _appStates;
        private readonly IDataFactory _dataFactory;

        private IImmutableList<IEntity> GetList()
	    {
            Configuration.Parse();

            // try to load the content-type - if it fails, return empty list
            if (string.IsNullOrWhiteSpace(ContentTypeName)) return EmptyList;

	        var useStream = TryToUseInStream == ContentTypeName && In.ContainsKey(StreamDefaultName);
	        var optionalList = useStream
	            ? In[StreamDefaultName]?.List.ToImmutableList()
	            : null;

	        var type = useStream 
                ? optionalList?.FirstOrDefault()?.Type
                : _appStates.Get(this).GetContentType(ContentTypeName);

	        // try to load from type, if it exists
	        var list = type?.Attributes?
                .OrderBy(at => at.Name)
                .Select(at => AsDic(at.Name, at.Type, at.IsTitle, at.SortOrder, false))
                .ToList();

            // note that often dynamic types will have zero attributes, so the previous command
            // gives a 0-list of attributes
            // so if that's the case, check the first item in the results
	        if (list == null || list.Count == 0)
	            list = TryToUseInStream == ContentTypeName
	                ? optionalList?.FirstOrDefault()?.Attributes
	                    .OrderBy(at => at.Key)
	                    .Select(at => AsDic(at.Key, at.Value.Type, false, 0, false))
	                    .ToList()
	                : null;

            // New 2022-10-17 2dm - also add Id, Created, Modified etc.
            if (list != null)
                foreach (var sysField in Data.Attributes.SystemFields.Reverse())
                    if (!list.Any(dic =>
                            dic.TryGetValue(AttributeType.Name.ToString(), out var name) &&
                            name as string == sysField.Key))
                        list.Insert(0, AsDic(sysField.Key, ValueTypeHelpers.Get(sysField.Value), false, 0, true));

            // if it didn't work yet, maybe try from stream items
            return list?.Select(attribData => _dataFactory.Create(attribData)).ToImmutableList()
                   ?? EmptyList;
        }

        private static Dictionary<string, object> AsDic(string name, ValueTypes type, bool isTitle, int sortOrder,
            bool builtIn)
            => new Dictionary<string, object>
            {
                { AttributeType.Name.ToString(), name },
                { AttributeType.Type.ToString(), type.ToString() },
                { AttributeType.IsTitle.ToString(), isTitle },
                { AttributeType.SortOrder.ToString(), sortOrder },
                { AttributeType.IsBuiltIn.ToString(), builtIn },
                { AttributeType.Title.ToString(), $"{name} ({type}{(builtIn ? ", built-in" : "")})" }
            };
    }
}