using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Repository.Efc.Tests
{
    [TestClass]
    public class MergeEntitiesTests
    {

        // Todo
        // finish implementing ML-handling and test every possible case
        // check relationship handling
        // handle special properties like ispublished, etc.
        // --> todo: ensure the guid doesn't update/change the guid! - in the save-layer...
        // handle missing guid in the save layer?
        // move the special save-setting in the Entity into the SaveOptions...

        #region Test Data Person tests

        private const int AppId = Constants.TransientAppId;
        //ContentType _ctNull = null;

        ContentType _ctPerson = new ContentType(AppId, "Person") {Attributes = new List<IContentTypeAttribute>
        {
            new ContentTypeAttribute(AppId, "FullName", "String", true, 0, 0),
            new ContentTypeAttribute(AppId, "FirstName", "String", true, 0, 0),
            new ContentTypeAttribute(AppId, "LastName", "String", true, 0, 0),
            new ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
            new ContentTypeAttribute(AppId, "Husband", "String", true, 0, 0),
            new ContentTypeAttribute(AppId, "UnusedField", "String", true, 0,0)
        }};
        Entity _origENull = null;

        private Entity GirlSingle => new Entity(AppId, 999, _ctPerson, new Dictionary<string, object>
        {
            {"FullName", "Sandra Unmarried"},
            {"FirstName", "Sandra"},
            {"LastName", "Unmarried"},
            {"Birthday", new DateTime(1981, 5, 14) }
        });

        private Entity GirlMarried => new Entity(AppId, 0, ContentTypeBuilder.Fake("DynPerson"), new Dictionary<string, object>
        {
            {"FullName", "Sandra Unmarried-Married"},
            {"FirstName", "Sandra"},
            {"LastName", "Unmarried-Married"},
            {"Husband", "HusbandName" },
            {"SingleName", "Unmarried" },
            {"WeddingDate", DateTime.Today }
        });

        private Entity GirlMarriedUpdate => new Entity(AppId, 0, _ctPerson, new Dictionary<string, object>
        {
            {"FullName", "Sandra Unmarried-Married"},
            //{"FirstName", "Sandra"},
            {"LastName", "Unmarried-Married"},
            {"Husband", "HusbandName" },
            {"SingleName", "Unmarried" },
            {"WeddingDate", DateTime.Today }
        });



        #endregion

        #region languge definitions
        private static Language langEn = new Language {DimensionId = 1, Key = "en-US"};
        private static Language langDeDe = new Language {DimensionId = 42, Key = "de-DE"};
        private static Language langDeCh = new Language {DimensionId = 39, Key = "de-CH"};
        private static Language langFr = new Language {DimensionId = 99, Key = "fr-FR"};

        private static DimensionDefinition langEnDef = new DimensionDefinition { DimensionId = 1, EnvironmentKey = "en-US" };
        private static DimensionDefinition langDeDeDef = new DimensionDefinition { DimensionId = 42, EnvironmentKey = "de-DE" };
        private static DimensionDefinition langDeChDef = new DimensionDefinition { DimensionId = 39, EnvironmentKey = "de-CH" };
        private static DimensionDefinition langFrDef = new DimensionDefinition { DimensionId = 99, EnvironmentKey = "fr-FR" };
        private static List<DimensionDefinition> activeLangs = new List<DimensionDefinition> { langEnDef, langDeDeDef, langDeChDef };

        #endregion

        #region build SaveOptions as needed
        readonly SaveOptions _saveDefault = CreateOptions();
        readonly SaveOptions _saveKeepAttribs = CreateOptions(PreserveExistingAttributes: true);
        readonly SaveOptions _saveKeepAndClean = CreateOptions(PreserveExistingAttributes: true, PreserveUnknownAttributes: false);
        readonly SaveOptions _saveClean = CreateOptions( PreserveUnknownAttributes: false);
        readonly SaveOptions _saveKeepUnknownLangs = CreateOptions( PreserveUnknownLanguages: true);
        readonly SaveOptions _saveKeepExistingLangs = CreateOptions( PreserveExistingLanguages: true);
        readonly SaveOptions _saveSkipExisting = CreateOptions(SkipExistingAttributes:true, PreserveUnknownAttributes:true);
        private static SaveOptions CreateOptions(bool? PreserveExistingAttributes = null, bool? PreserveUnknownAttributes = null, bool? PreserveUnknownLanguages = null, bool? PreserveExistingLanguages = null, bool? SkipExistingAttributes = null)
        {
            var x = new SaveOptions(langEn.Key, activeLangs);
            if (PreserveExistingAttributes != null)
                x.PreserveUntouchedAttributes = PreserveExistingAttributes.Value;
            if (PreserveUnknownAttributes != null)
                x.PreserveUnknownAttributes = PreserveUnknownAttributes.Value;
            if (PreserveUnknownLanguages != null)
                x.PreserveUnknownLanguages = PreserveUnknownLanguages.Value;
            if (PreserveExistingLanguages != null)
                x.PreserveExistingLanguages = PreserveExistingLanguages.Value;
            if (SkipExistingAttributes != null)
                x.SkipExistingAttributes = SkipExistingAttributes.Value;
            return x;
        }

        #endregion

        #region Test Data ML

        private readonly ContentType _ctMlProduct = new ContentType(-1, "Product")
        {
            Attributes = new List<IContentTypeAttribute>
            {
                new ContentTypeAttribute(AppId, "Title", "String", true, 0, 0),
                new ContentTypeAttribute(AppId, "Teaser", "String", false, 0, 0),
                new ContentTypeAttribute(AppId, "Image", "Hyperlink", false, 0, 0),
            }
        };

        private readonly Entity _prodNull = null;
        private Entity ProdNoLang => new Entity(AppId, 3006, _ctMlProduct, new Dictionary<string, object>()
        {
            { "Title", "Original Product No Lang" },
            { "Teaser", "Original Teaser no lang" },
            { "Image", "file:403" }
        }, "Title");

        private readonly Entity _prodEn = ((Func<Entity>)(() =>
        {
            var title = AttributeBuilder.CreateTyped("Title", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "TitleEn, language En", new List<ILanguage> {langEn.Copy()}),
            });
            var teaser = AttributeBuilder.CreateTyped("Teaser", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "Teaser EN, lang en", new List<ILanguage> {langEn.Copy()}),
            });
            var file = AttributeBuilder.CreateTyped("File", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "File EN, lang en + ch RW", new List<ILanguage> {langEn.Copy() }),
            });

            return new Entity(AppId, 3006, ContentTypeBuilder.Fake("Product"), new Dictionary<string, object>
            {
                {title.Name, title},
                {teaser.Name, teaser},
                {file.Name, file}
            }, "Title");
        }))();

        private readonly Entity _prodMl  = ((Func<Entity>)(() =>
        {
            var title = AttributeBuilder.CreateTyped("Title", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "TitleEn, language En", new List<ILanguage> {langEn.Copy()}),
                ValueBuilder.Build(ValueTypes.String, "Title DE",
                    new List<ILanguage> {langDeDe.Copy(), langDeCh.Copy(readOnly: true)}),
                ValueBuilder.Build(ValueTypes.String, "titre FR", new List<ILanguage> {langFr.Copy()})
            });

            var teaser = AttributeBuilder.CreateTyped("Teaser", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "teaser de de",
                    new List<ILanguage> {langDeDe.Copy() }),
                ValueBuilder.Build(ValueTypes.String, "teaser de CH",
                    new List<ILanguage> {langDeCh.Copy()}),
                ValueBuilder.Build(ValueTypes.String, "teaser FR", new List<ILanguage> {langFr.Copy( readOnly:true)}),
                // special test: leave EN (primary) at end of list, as this could happen in real life
                ValueBuilder.Build(ValueTypes.String, "Teaser EN, lang en", new List<ILanguage> {langEn.Copy()}),
            });
            var file = AttributeBuilder.CreateTyped("File", "String", new List<IValue>
            {
                ValueBuilder.Build(ValueTypes.String, "Filen EN, lang en + ch RW", new List<ILanguage> {langEn.Copy(), langDeCh.Copy()}),
                ValueBuilder.Build(ValueTypes.String, "File de de",
                    new List<ILanguage> {langDeDe.Copy(), langFr.Copy() }),
                ValueBuilder.Build(ValueTypes.String, "File FR", new List<ILanguage> {langFr.Copy()}),
                // special test - empty language item
                ValueBuilder.Build(ValueTypes.String, "File without language!", new List<ILanguage>()),
                ValueBuilder.Build(ValueTypes.String, "Filen EN, lang en + ch RW", new List<ILanguage> {langEn.Copy(), langDeCh.Copy()}),
            });

            return new Entity(AppId, 430, ContentTypeBuilder.Fake("Product"), new Dictionary<string, object>
            {
                {title.Name, title},
                {teaser.Name, teaser},
                {file.Name, file}
            }, "Title");
        }))();





        #endregion

        #region Test various cases of Multi-Language merge
        [TestMethod]
        public void MergeNoLangIntoNull_EnsureLangsDontGetAdded()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodNull, ProdNoLang, _saveDefault);

            Assert.AreEqual(1, merged["Title"].Values.Count, "should only have 1");
            var firstVal = merged["Title"].Values.First();
            Assert.AreEqual(0, firstVal.Languages.Count, "should still have no languages");
        }

        #region Test Multilanguage - for clearing / not clearing unknown languages
        [TestMethod]
        public void MergeMlIntoNoLang_MustClearUnknownLang()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(ProdNoLang, _prodMl, _saveDefault);

            Assert.AreEqual(2, merged["Title"].Values.Count, "should only have 2, no FR");
            var deVal = merged["Title"].Values.First(v => v.Languages.Any(l => l.Key == langDeDe.Key));
            Assert.AreEqual(2, deVal.Languages.Count, "should have 2 language");
        }

        [TestMethod]
        public void MergeMlIntoNoLang_DontClearUnknownLang()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(ProdNoLang, _prodMl, _saveKeepUnknownLangs);

            Assert.AreEqual(3, merged["Title"].Values.Count, "should have 3, with FR");
            var deVal = merged["Title"].Values.First(v => v.Languages.Any(l => l.Key == langFr.Key));
            Assert.AreEqual(1, deVal.Languages.Count, "should have 1 language");
        }
        #endregion

        #region More Tests Multilanguage
        [TestMethod]
        public void MergeEnIntoNoLang_ShouldHaveLang()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(ProdNoLang, _prodEn, _saveDefault);

            Assert.AreEqual(1, merged["Title"].Values.Count, "should only have 1");
            var firstVal = merged["Title"].Values.First();
            Assert.AreEqual(1, firstVal.Languages.Count, "should have 1 language");
        }

        [TestMethod]
        public void MergeEnIntoML_KeepLangs()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodMl, _prodEn, _saveKeepExistingLangs);

            // check the titles as expected
            Assert.AreEqual(2, merged["Title"].Values.Count, "should have 2 titles with languages - EN and a shared DE+CH");
            Assert.AreEqual(2, merged["Title"].Values.Single(v => v.Languages.Any(l => l.Key == langDeDe.Key)).Languages.Count, "should have 2 languages on the shared DE+CH");
            Assert.AreEqual(_prodEn.GetBestValue("Title").ToString(), merged.GetBestValue("Title", new[] { langEn.Key }), "en title should be the en-value");
            Assert.AreEqual(_prodMl.GetBestValue("Title", new [] {langDeDe.Key}).ToString(), merged.GetBestValue("Title", new[] { langDeDe.Key }), "de title should be the ML-value");
            Assert.AreEqual(_prodMl.GetBestValue("Title", new [] {langDeCh.Key}).ToString(), merged.GetBestValue("Title", new[] { langDeCh.Key }), "ch title should be the ML-value");
            Assert.AreNotEqual(_prodEn.GetBestValue("Title", new [] {langDeCh.Key}).ToString(), merged.GetBestValue("Title", new[] { langDeCh.Key }).ToString(), "ch title should be the ML-value");
            var firstVal = merged["Title"].Values.First();
            Assert.AreEqual(1, firstVal.Languages.Count, "should have 1 language");
            Assert.AreEqual(langEn.Key, firstVal.Languages.First().Key, "language should be EN-US");


        }

        public void MergeMlIntoEn_KeepLangs()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodMl, _prodEn, _saveKeepExistingLangs);

            // check the titles as expected
            Assert.AreEqual(2, merged["Title"].Values.Count, "should have 2 titles with languages - EN and a shared DE+CH");
        }

        // todo!
        [TestMethod]
        public void MergeMlIntoNoLang_KeepLangs()
        {
            //this test will have to merge ML into the no-lang, and ensure that no-lang is typed correctly!
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodEn, _prodMl, _saveKeepUnknownLangs);

            // todo!
        }

        [TestMethod]
        public void MergeNoLangIntoEn_DefaultNoMerge()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodEn, ProdNoLang, _saveDefault);

            Assert.AreEqual(1, merged.Title.Values.Count, "should only have 1");
            var firstVal = merged.Title.Values.First();
            Assert.AreEqual(0, firstVal.Languages.Count, "should not have languages left");
        }

        [TestMethod]
        public void MergeNoLangIntoEn_Merge()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodEn, ProdNoLang, _saveKeepExistingLangs);

            Assert.AreEqual(1, merged.Title.Values.Count, "should only have 1");
            var firstVal = merged.Title.Values.First();
            Assert.AreEqual(0, firstVal.Languages.Count, "should not have languages left");
        }

        [TestMethod]
        public void MergMlintoEnDefault_shouldBeMl()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_prodEn, _prodMl, _saveDefault);
            // todo: add some test conditions
        }
        #endregion

        #endregion

        #region Basic Merges - especially counting attributes and correct x-fer of primary attrib
        [TestMethod]
        public void MergeNullAndMarried()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(_origENull, GirlMarried, _saveDefault);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(GirlMarried.Attributes.Count, merged.Attributes.Count, "this test case should simply keep all values");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlMarried);
            Assert.AreNotSame(GirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");

            Assert.AreEqual(merged.GetBestValue("FullName"), GirlMarried.GetBestValue("FullName"), "full name should be that of married");
        }

        [TestMethod]
        public void MergeSingleAndMarried()
        {
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveDefault);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(GirlSingle.Attributes.Count, merged.Attributes.Count, "this test case should keep all values of the first type");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);
            Assert.AreNotSame(GirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same");
            Assert.AreEqual(merged.GetBestValue("FullName"), GirlMarried.GetBestValue("FullName"), "full name should be that of married");
            Assert.AreNotEqual(merged.GetBestValue("FullName"), GirlSingle.GetBestValue("FullName"), "full name should be that of married");

            // Merge keeping 
            merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveKeepAttribs);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreNotEqual(GirlSingle.Attributes.Count, merged.Attributes.Count, "should have more than original count");
            Assert.AreNotEqual(GirlMarried.Attributes.Count, merged.Attributes.Count, "should have more than new count");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);
            Assert.AreNotSame(GirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");

            // Merge updating only 
            merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarriedUpdate, _saveKeepAttribs);
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreNotEqual(GirlSingle.Attributes.Count, merged.Attributes.Count, "should have more than original count");
            Assert.AreNotEqual(GirlMarried.Attributes.Count, merged.Attributes.Count, "should have more than new count");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);
            Assert.AreNotSame(GirlMarried.Attributes, merged.Attributes, "attributes new / merged shouldn't be same object in this case");

        }
        [TestMethod]
        public void MergeSingleAndMarriedFilterCtAttribs()
        {
            //GirlSingle.SetType(_ctPerson);
            // Merge keeping all and remove unknown attributes
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveKeepAndClean);
            var expectedFields = new List<string> { "FullName", "FirstName", "LastName", "Birthday", "Husband" };
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(expectedFields.Count, merged.Attributes.Count, "should have only ct-field count except the un-used one");
            Assert.AreEqual(0, expectedFields.Except(merged.Attributes.Keys).Count(), "should have exactly the same fields as expected");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);

            // Merge keeping all and remove unknown attributes
            merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveClean);
            expectedFields.Remove("Birthday");
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(expectedFields.Count, merged.Attributes.Count, "should have only ct-field count except the un-used one");
            Assert.AreEqual(0, expectedFields.Except(merged.Attributes.Keys).Count(), "should have exactly the same fields as expected");
            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);

        }

        [TestMethod]
        public void Merge_SkipExisting()
        {
            //GirlSingle.SetType(_ctPerson);
            // Merge keeping all and remove unknown attributes
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveSkipExisting);
            // var expectedFields = new List<string> {"FullName", "FirstName", "LastName", "Birthday", "Husband"};
            Assert.IsNotNull(merged, "result should never be null");
            Assert.AreEqual(GirlSingle.GetBestValue("FullName"), merged.GetBestValue("FullName"), "should keep single name");
            Assert.AreEqual(GirlSingle.GetBestValue("LastName"), merged.GetBestValue("LastName"), "should keep single name");
            Assert.AreEqual(GirlMarried.GetBestValue("Husband"), merged.GetBestValue("Husband"), "should keep single name");
        }

        [TestMethod]
        public void MergeSingleAndMarriedFilterUnknownCt()
        {
            // Merge keeping all and remove unknown attributes
            var merged = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlSingle, GirlMarried, _saveKeepAndClean);
            var expectedFields = GirlSingle.Attributes.Keys;// GirlSingle.Attributes.Keys.Concat(GirlMarried.Attributes.Keys).Distinct().ToList();
            Assert.IsNotNull(merged, "result should never be null");
            Assert.IsTrue(expectedFields.Count <= merged.Attributes.Count 
                && GirlSingle.Type.Attributes.Count >= merged.Attributes.Count, "should have only ct-field count except the un-used one");
            Assert.AreEqual(0, expectedFields.Except(merged.Attributes.Keys).Count(), "should have exactly the same fields as expected");

            var merged2 = new EntitySaver(new  Log("Tst.Merge")).CreateMergedForSaving(GirlMarried, GirlSingle, _saveKeepAndClean);
            var expectedFields2 = GirlMarried.Attributes.Keys.Concat(GirlSingle.Attributes.Keys).Distinct().ToList();
            Assert.IsNotNull(merged2, "result should never be null");
            Assert.AreEqual(expectedFields2.Count, merged2.Attributes.Count, "should have only ct-field count except the un-used one");
            Assert.AreEqual(0, expectedFields2.Except(merged2.Attributes.Keys).Count(), "should have exactly the same fields as expected");


            AssertBasicsInMerge(_origENull, GirlMarried, merged, GirlSingle);
        }

        private static void AssertBasicsInMerge(IEntity orig, IEntity newE, IEntity merged, IEntity stateProviderE)
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

        #endregion
    }
}
