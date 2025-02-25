﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Internal;
using ToSic.Eav.DataSource.Internal.Configuration;
using ToSic.Eav.DataSources;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests;

[TestClass]
public class ConfigMaskAuto: TestBaseEavDataSource
{
    [TestMethod]
    public void EnsureNoCacheAtFirstAndCacheLater()
    {
        // Make sure it's reset
        ConfigurationDataLoader.Cache = new ConcurrentDictionary<Type, List<ConfigMaskInfo>>();

        // should be false at first
        IsFalse(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
        var findConfigs = GetService<ConfigurationDataLoader>();
        var masks = findConfigs.GetTokensTac(typeof(Shuffle));
        IsTrue(ConfigurationDataLoader.Cache.ContainsKey(typeof(Shuffle)));
    }

    [TestMethod]
    public void GetMasksForShuffle()
    {
        var findConfigs = GetService<ConfigurationDataLoader>();
        var masks = findConfigs.GetTokensTac(typeof(Shuffle));
        AreEqual(1, masks.Count);
        AreEqual($"[{DataSourceConstants.MyConfigurationSourceName}:Take||0]", masks.First().Token);
    }

    [TestMethod]
    public void GetMasksForInheritedDataSource()
    {
        var findConfigs = GetService<ConfigurationDataLoader>();
        var masks = findConfigs.GetTokensTac(typeof(Children));
        AreEqual(3, masks.Count);
        AreEqual($"[{DataSourceConstants.MyConfigurationSourceName}:{nameof(Children.FieldName)}]", masks.Skip(1).First().Token);
    }

    [TestMethod]
    public void GetMasksWithOtherFieldName()
    {
        var findConfigs = GetService<ConfigurationDataLoader>();
        var masks = findConfigs.GetTokensTac(typeof(RelationshipFilter));
        AreEqual(6, masks.Count);
        AreEqual($"[{DataSourceConstants.MyConfigurationSourceName}:Direction||{RelationshipFilter.DefaultDirection}]", masks.First(m => m.Key == nameof(RelationshipFilter.ChildOrParent)).Token);
    }

    [TestMethod]
    public void GetMasksWithEnumDefault()
    {
        var findConfigs = GetService<ConfigurationDataLoader>();
        var masks = findConfigs.GetTokensTac(typeof(RelationshipFilter));
        AreEqual(6, masks.Count);
        AreEqual($"[{DataSourceConstants.MyConfigurationSourceName}:Comparison||contains]", masks.First(m => m.Key == nameof(RelationshipFilter.CompareMode)).Token);
    }

    [TestMethod]
    public void ShuffleShouldHave1Mask()
    {
        var shuffle = CreateDataSource<Shuffle>();

        AreEqual(1, shuffle.Configuration.Values.Count);
    }

}