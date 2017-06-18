using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repository.Efc.Parts;
using ToSic.Eav.Repository.Efc.Tests.Mocks;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class SaveEntitiesTests
    {
        #region Test Data
        
        ContentType CtNull = null;
        ContentType CtPerson = new ContentType("Person", "Person") {Attributes = new List<IAttributeDefinition>()
        {
            new AttributeDefinition("FullName", "String", true, 0, 0),
            new AttributeDefinition("FirstName", "String", true, 0, 0),
            new AttributeDefinition("LastName", "String", true, 0, 0),
            new AttributeDefinition("Birthday", "DateTime", true, 0, 0),
            new AttributeDefinition("Husband", "String", true, 0, 0),
        }};
        Entity OrigENull = null;

        private readonly Interfaces.IEntity oGirlSingle = new Entity(999, "", new Dictionary<string, object>()
        {
            {"FullName", "Sandra Unmarried"},
            {"FirstName", "Sandra"},
            {"LastName", "Unmarried"},
            {"Birthday", new DateTime(1981, 5, 14) }
        });

        private readonly Interfaces.IEntity nGirlMarried = new Entity(0, "", new Dictionary<string, object>()
        {
            {"FullName", "Sandra Unmarried-Married"},
            {"FirstName", "Sandra"},
            {"LastName", "Unmarried-Married"},
            {"Husband", "HusbandName" },
            {"SingleName", "Unmarried" },
            {"WeddingDate", DateTime.Today }
        });

        private readonly Interfaces.IEntity nGirlMarriedUpdate = new Entity(0, "", new Dictionary<string, object>()
        {
            {"FullName", "Sandra Unmarried-Married"},
            //{"FirstName", "Sandra"},
            {"LastName", "Unmarried-Married"},
            {"Husband", "HusbandName" },
            {"SingleName", "Unmarried" },
            {"WeddingDate", DateTime.Today }
        });

        readonly SaveOptions saveOptionsDefault = new SaveOptions();
        readonly SaveOptions saveOptionsKeep = new SaveOptions { PreserveExistingAttributes = true};
        private readonly SaveOptions saveOptionsCleanByCt = new SaveOptions {RemoveUnknownAttributes = true};

        #endregion

        [TestMethod]
        public void MergeNullAndMarried()
        {
            var merged = EntitySaver.CreateMergedForSaving(OrigENull, nGirlMarried, CtNull, saveOptionsDefault);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(nGirlMarried.Attributes.Count, merged.Attributes.Count, "this test case should simply keep all values");
            AssertBasicsInMerge(OrigENull, nGirlMarried, merged, nGirlMarried);
            Assert.AreSame(nGirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");
        }

        [TestMethod]
        public void MergeSingleAndMarried()
        {
            var merged = EntitySaver.CreateMergedForSaving(oGirlSingle, nGirlMarried, CtNull, saveOptionsDefault);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(nGirlMarried.Attributes.Count, merged.Attributes.Count, "this test case should simply keep all new values");
            AssertBasicsInMerge(OrigENull, nGirlMarried, merged, oGirlSingle);
            Assert.AreNotSame(nGirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same");

            // Merge keeping 
            merged = EntitySaver.CreateMergedForSaving(oGirlSingle, nGirlMarried, CtNull, saveOptionsKeep);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreNotEqual(oGirlSingle.Attributes.Count, merged.Attributes.Count, "should have more than original count");
            Assert.AreNotEqual(nGirlMarried.Attributes.Count, merged.Attributes.Count, "should have more than new count");
            AssertBasicsInMerge(OrigENull, nGirlMarried, merged, oGirlSingle);
            Assert.AreNotSame(nGirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");

            // Merge updating only 
            merged = EntitySaver.CreateMergedForSaving(oGirlSingle, nGirlMarriedUpdate, CtNull, saveOptionsKeep);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreNotEqual(oGirlSingle.Attributes.Count, merged.Attributes.Count, "should have more than original count");
            Assert.AreNotEqual(nGirlMarried.Attributes.Count, merged.Attributes.Count, "should have more than new count");
            AssertBasicsInMerge(OrigENull, nGirlMarried, merged, oGirlSingle);
            Assert.AreNotSame(nGirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");

        }
        [TestMethod]
        public void MergeSingleAndMarriedFilterCtAttribs()
        {
            // todo: merge with type definition filter but without type
            // Merge keeping 
            var merged = EntitySaver.CreateMergedForSaving(oGirlSingle, nGirlMarried, CtPerson, saveOptionsCleanByCt);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(CtPerson.Attributes.Count, merged.Attributes.Count, "should have only ct-field count");
            AssertBasicsInMerge(OrigENull, nGirlMarried, merged, oGirlSingle);

        }

        private static void AssertBasicsInMerge(Interfaces.IEntity orig, Interfaces.IEntity newE, Interfaces.IEntity merged, Interfaces.IEntity stateProviderE)
        {
            // make sure we really created a new object and that it's not identical to one of the originals
            Assert.AreNotSame(orig, merged, "merged shouldn't be original");
            Assert.AreNotSame(newE, merged, "merged shouldn't be new-data item");

            // make sure identity etc. are based on the identity-providing item
            Assert.AreEqual(stateProviderE.EntityId, merged.EntityId, "entityid");
            Assert.AreEqual(stateProviderE.EntityGuid, merged.EntityGuid, "guid");
            Assert.AreEqual(stateProviderE.IsPublished, merged.IsPublished, "ispublished");
            Assert.AreEqual(stateProviderE.RepositoryId, merged.RepositoryId, "repositoryid");
            Assert.AreEqual(stateProviderE.GetDraft(), merged.GetDraft(), "getdraft()");

        }
    }
}
