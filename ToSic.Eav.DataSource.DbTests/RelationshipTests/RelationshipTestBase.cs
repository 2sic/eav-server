using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSource.DbTests.RelationshipTests;

public class RelationshipTestBase(DataSourcesTstBuilder dsSvc, DataBuilder dataBuilder)
{
    #region Const Values for testing

    public const string CompCat = "Categories";
    public const string CatWeb = "Web";
    public const string CatGreen = "Green";
    public const string CompInexistingProp = "InexistingProperty";


    protected const string DefSeparator = ",";
    protected const string AltSeparator = "|";

    #endregion

    protected new ILog Log { get; } = new Log("Tst.DSRelF");




    /// <summary>
    /// Build a relationship filter and configure it using the API
    /// </summary>
    /// <returns></returns>
    protected RelationshipFilter FilterWithApi(string type, 
        string relationship = null, 
        string filter = null, 
        string relAttrib = null,
        string compareMode = null,
        string separator = null,
        string direction = null)
    {
        var relApi = BuildRelationshipFilter(type);

        if (relationship != null) relApi.Relationship = relationship;
        if (filter != null) relApi.Filter = filter;
        if (relAttrib != null) relApi.CompareAttribute = relAttrib;
        if (compareMode != null) relApi.CompareMode = compareMode;
        if (separator != null) relApi.Separator = separator;
        if (direction != null) relApi.ChildOrParent = direction;

        return relApi;
    }

    /// <summary>
    /// Build a filter using a configuration/settings object to provide configuration
    /// </summary>
    /// <returns></returns>
    protected RelationshipFilter FilterWithConfig(string type, 
        string relationship = null, 
        string filter = null,
        string relAttrib = null,
        string compareMode = null,
        string separator = null,
        string direction = null)
    {
        var settings = new Dictionary<string, object> { { Attributes.TitleNiceName, "..." } };

        void MaybeAddValueStr(string key, string value) { if (value != null) settings.Add(key, value); }
        MaybeAddValueStr(nameof(RelationshipFilter.Relationship), relationship);
        MaybeAddValueStr(nameof(RelationshipFilter.Filter), filter);
        MaybeAddValueStr(RelationshipFilter.FieldAttributeOnRelationship, relAttrib);
        MaybeAddValueStr(RelationshipFilter.FieldComparison, compareMode);
        MaybeAddValueStr(nameof(RelationshipFilter.Separator), separator);
        MaybeAddValueStr(RelationshipFilter.FieldDirection, direction);

        var config = BuildConfigurationProvider(settings);
        return BuildRelationshipFilter(RelationshipTestSpecs.Company, config);
    }




    protected RelationshipFilter BuildRelationshipFilter(string primaryType, ILookUpEngine config = null)
    {
        var baseDs = dsSvc.DataSourceSvc.CreateDefault(new DataSourceOptions
            {
                AppIdentityOrReader = RelationshipTestSpecs.AppIdentity,
                LookUp = config
            }
        );
        var appDs = dsSvc.CreateDataSource<App>(baseDs);

        // micro tests to ensure we have the right app etc.
        True(appDs.ListTac().Count() > 20, "appDs.List.Count() > 20");

        var item731 = appDs.ListTac().FindRepoId(731);
        NotNull(item731);//, "expecting item 731");
        var title = item731.GetBestTitle();
        Equal(title, "2sic");//, "item 731 should have title '2sic'");

        True(appDs.Out.ContainsKey(primaryType), $"app should contain stream of {primaryType}");

        var stream = appDs[primaryType];

        True(stream.ListTac().Any(), "stream.List.Count() > 0");

        var relFilt = dsSvc.CreateDataSource<RelationshipFilter>(appDs.Configuration.LookUpEngine);
        relFilt.AttachTac(DataSourceConstants.StreamDefaultName, stream);
        return relFilt;
    }


    protected LookUpEngine BuildConfigurationProvider(Dictionary<string, object> vals)
    {
        var testData = new LookUpTestData(dataBuilder);
        var lookup = testData.BuildLookUpEntity(DataSourceConstants.MyConfigurationSourceName, vals);
        var vc = testData.AppSetAndRes(sources: [lookup]);
        return vc;
    }

}