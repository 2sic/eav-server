using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Repositories;
using ToSic.Eav.Repository.Efc.Tests;
using ToSic.Testing.Shared;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class XmlSerializationTests: TestBaseDiEavFullAndDb
    {
        private readonly XmlSerializer _xmlSerializer;
        public XmlSerializationTests()
        {
            _xmlSerializer = GetService<XmlSerializer>();
        }

        //private int AppId = 2;
        //private int TestItemId = 0;

        [TestMethod]
        public void Xml_SerializeItemOnHome()
        {
            var test = new SpecsTestExportSerialize();
            var app = GetService<IRepositoryLoader>().AppStateReaderRawTA(test.AppId);
            //var zone = new ZoneRuntime().Init(test.ZoneId, Log);
            var languageMap = GetService<IAppStates>().Languages(test.ZoneId)
                .ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId);
            var exBuilder = _xmlSerializer.Init(languageMap, app);
            var xmlEnt = exBuilder.Serialize(test.TestItemToSerialize);
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long xml string");
            Trace.Write(xmlEnt);
            //Assert.AreEqual(xmlstring, xmlEnt, "xml strings should be identical");
        }

        [Ignore]
        [TestMethod]
        public void Xml_CompareAllSerializedEntitiesOfApp()
        {
            var test = new SpecsTestExportSerialize();
            var appId = test.AppId;
            var app = GetService<IRepositoryLoader>().AppStateReaderRawTA(appId);
            var languageMap = GetService<IAppStates>().Languages(test.ZoneId).ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId);
            var exBuilder = _xmlSerializer.Init(languageMap, app);

            var maxCount = 500;
            var skip = 0;
            var count = 0;
            try
            {
                foreach (var appEntity in app.List)
                {
                    // maybe skip some
                    if (count++ < skip) continue;

                    //var xml = xmlbuilder.XmlEntity(appEntity.EntityId);
                    //var xmlstring = xml.ToString();
                    var xmlEnt = exBuilder.Serialize(appEntity.EntityId);
                    //Assert.AreEqual(xmlstring, xmlEnt,
                    //    $"xml of item {count} strings should be identical - but was not on xmlEnt {appEntity.EntityId}");

                    // stop if we ran enough tests
                    if (count >= maxCount)
                        return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"had issue after count{count}", ex);
            }


        }
    }


}
