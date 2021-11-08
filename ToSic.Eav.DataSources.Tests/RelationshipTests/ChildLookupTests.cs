using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Core.Tests.LookUp;
using ToSic.Eav.DataSources;
using ToSic.Eav.LookUp;
using ToSic.Testing.Shared;
using static ToSic.Eav.DataSourceTests.RelationshipTests.RelationshipTestSpecs;

namespace ToSic.Eav.DataSourceTests.RelationshipTests
{
    [TestClass]
    public class ChildLookupTests: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void PersonsAllWithoutFieldReturnAllCompanies()
        {
            var cl = GetChildLookup(Person, null, Company);
            Assert.AreEqual(3, cl.ListForTests().Count());
        }

        [TestMethod]
        public void PersonsOneGetOneCompany()
        {
            var cl = GetChildLookup(Person, new []{ PersonWithCompany }, Company);
            Assert.AreEqual(PersonCompanyCount, cl.ListForTests().Count());
        }

        [TestMethod]
        public void CompanyOneHas5Children()
        {
            var cl = GetChildLookup(Company, new []{ CompanyIdWithCountryAnd4Categories });
            Assert.AreEqual(5, cl.ListForTests().Count());
        }
        [TestMethod]
        public void CompanyOneHas4Categories()
        {
            var cl = GetChildLookup(Company, new []{ CompanyIdWithCountryAnd4Categories }, Categories);
            Assert.AreEqual(4, cl.ListForTests().Count());
        }
        [TestMethod]
        public void CompanyOneHas1Country()
        {
            var cl = GetChildLookup(Company, new []{ CompanyIdWithCountryAnd4Categories }, Country);
            Assert.AreEqual(1, cl.ListForTests().Count());
        }


        [TestMethod]
        public void InButNoFieldNameReturnLotsOfChildren() => Assert.IsTrue(GetChildLookup().ListForTests().Count() > 20);


        private Children GetChildLookup(string appType = null, IEnumerable<int> ids = null, string fieldName = null, ILookUpEngine lookUpEngine = null)
        {
            if(lookUpEngine == null) lookUpEngine = LookUpTestData.AppSetAndRes();

            var baseDs = DataSourceFactory.GetPublishing(AppIdentity, configProvider: lookUpEngine);
            var appDs = DataSourceFactory.GetDataSource<App>(baseDs);
            var inStream = appDs.GetStream(appType);
            if (ids != null && ids.Count() > 0)
            {
                var entityFilterDs = DataSourceFactory.GetDataSource<EntityIdFilter>(inStream);
                entityFilterDs.EntityIds = string.Join(",", ids);
                inStream = entityFilterDs.GetStream();
            }


            var childDs = DataSourceFactory.GetDataSource<Children>(inStream);
            //childDs.Attach(Constants.DefaultStreamName, appDs, appType ?? Constants.DefaultStreamName);

            if (fieldName != null) childDs.FieldName = fieldName;
            return childDs;
        }

    }
}
