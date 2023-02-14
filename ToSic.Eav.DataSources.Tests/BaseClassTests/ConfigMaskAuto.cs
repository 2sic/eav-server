﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests
{
    [TestClass]
    public class ConfigMaskAuto: TestBaseDiEavFullAndDb
    {
        [TestMethod]
        public void EnsureNoCacheAtFirstAndCacheLater()
        {
            IsFalse(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
            var findConfigs = Build<ConfigurationDataLoader>();
            var masks = findConfigs.GetTokens(typeof(Shuffle));
            IsTrue(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
        }

        [TestMethod]
        public void GetMasksForShuffle()
        {
            var findConfigs = Build<ConfigurationDataLoader>();
            var masks = findConfigs.GetTokens(typeof(Shuffle));
            AreEqual(1, masks.Count);
            AreEqual($"[{DataSource.MyConfiguration}:Take||0]", masks.First().Token);
        }

        [TestMethod]
        public void GetMasksForInheritedDataSource()
        {
            var findConfigs = Build<ConfigurationDataLoader>();
            var masks = findConfigs.GetTokens(typeof(Children));
            AreEqual(3, masks.Count);
            AreEqual($"[{DataSource.MyConfiguration}:{nameof(Children.FieldName)}]", masks.Skip(1).First().Token);
        }

        [TestMethod]
        public void GetMasksWithOtherFieldName()
        {
            var findConfigs = Build<ConfigurationDataLoader>();
            var masks = findConfigs.GetTokens(typeof(RelationshipFilter));
            AreEqual(6, masks.Count);
            AreEqual($"[{DataSource.MyConfiguration}:Direction||{RelationshipFilter.DefaultDirection}]", masks.First(m => m.Key == nameof(RelationshipFilter.ChildOrParent)).Token);
        }

        [TestMethod]
        public void GetMasksWithEnumDefault()
        {
            var findConfigs = Build<ConfigurationDataLoader>();
            var masks = findConfigs.GetTokens(typeof(RelationshipFilter));
            AreEqual(6, masks.Count);
            AreEqual($"[{DataSource.MyConfiguration}:Comparison||contains]", masks.First(m => m.Key == nameof(RelationshipFilter.CompareMode)).Token);
        }

        [TestMethod]
        public void ShuffleShouldHave1Mask()
        {
            var shuffle = this.GetTestDataSource<Shuffle>();

            AreEqual(1, shuffle.Configuration.Values.Count);
        }

    }
}
