using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests
{
    public class ChildParentTestBase<T> : TestBaseDiEavFullAndDb where T: RelationshipDataSourceBase
    {


        protected T PrepareDs(string appType = null, IEnumerable<int> ids = null, string fieldName = null, ILookUpEngine lookUpEngine = null)
        {
            if(lookUpEngine == null) lookUpEngine = LookUpTestData.AppSetAndRes();

            var baseDs = DataSourceFactory.GetPublishing(AppIdentity, configProvider: lookUpEngine);
            var appDs = DataSourceFactory.GetDataSource<App>(baseDs);
            var inStream = appDs.GetStream(appType);
            if (ids != null && ids.Any())
            {
                var entityFilterDs = DataSourceFactory.GetDataSource<EntityIdFilter>(inStream);
                entityFilterDs.EntityIds = string.Join(",", ids);
                inStream = entityFilterDs.GetStream();
            }

            var childDs = DataSourceFactory.GetDataSource<T>(inStream);

            if (fieldName != null) childDs.FieldName = fieldName;
            return childDs;
        }

    }
}
