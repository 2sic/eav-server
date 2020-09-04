﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Tests;

namespace ToSic.Eav.ImportExport.Tests
{
    [TestClass]
    public class XmlSerializationTests
    {
        public static ILog Log = new Log("TstXml");
        //private int AppId = 2;
        //private int TestItemId = 0;

        [TestMethod]
        public void Xml_SerializeItemOnHome()
        {
            var test = new TestValuesOnPc2Dm();
            var dbc = DbDataController.Instance(null, test.AppId, Log);
            //var xmlbuilder = new DbXmlBuilder(dbc);

            //var xml = xmlbuilder.XmlEntity(test.ItemOnHomeId);
            
            //var xmlstring = xml.ToString();
            //Assert.IsTrue(xmlstring.Length > 200, "should get a long xml string");

            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppState(test.AppId);
            var zone = new ZoneRuntime(test.ZoneId, Log);
            var languageMap = zone.Languages().ToDictionary(l => l.EnvironmentKey.ToLower(), l => l.DimensionId);
            var exBuilder = new XmlSerializer(languageMap);
            exBuilder.Initialize(app, Log);
            var xmlEnt = exBuilder.Serialize(test.ItemOnHomeId);
            Assert.IsTrue(xmlEnt.Length > 200, "should get a long xml string");
            Trace.Write(xmlEnt);
            //Assert.AreEqual(xmlstring, xmlEnt, "xml strings should be identical");
        }

        [Ignore]
        [TestMethod]
        public void Xml_CompareAllSerializedEntitiesOfApp()
        {
            var test = new TestValuesOnPc2Dm();
            var appId = test.BlogAppId;
            var dbc = DbDataController.Instance(null, appId, Log);
            var loader = new Efc11Loader(dbc.SqlDb);
            var app = loader.AppState(appId);
            var zone = new ZoneRuntime(test.ZoneId, Log);
            var languageMap = zone.Languages().ToDictionary(l => l.EnvironmentKey.ToLower(), l => l.DimensionId);
            var exBuilder = new XmlSerializer(languageMap);
            exBuilder.Initialize(app, Log);

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
