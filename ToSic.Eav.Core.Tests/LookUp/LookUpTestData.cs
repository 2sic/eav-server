﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Core.Tests.LookUp;

public class LookUpTestData(DataBuilder builder)
{
    public const string KeyAppSettings = "AppSettings";
    public const string KeyAppResources = "AppResources";

    private const int AppIdX = -1;

    public static LookUpEngine EmptyLookupEngine(List<ILookUp> sources = default)
        => new(null, sources: sources);

    public LookUpEngine AppSetAndRes(int appId = AppIdX, List<ILookUp> sources = default)
    {
        sources ??= [];
        var vc = EmptyLookupEngine(sources: sources.Concat(new List<ILookUp>
            {
                AppSettings(appId),
                AppResources(appId)
            }).ToList()
        );
        return vc;
    }

    public LookUpInEntity BuildLookUpEntity(string name, Dictionary<string, object> values, int appId = AppIdX)
    {
        var ent = builder.TestCreate(appId: appId, contentType: builder.ContentType.Transient(name), values: values, titleField: values.FirstOrDefault().Key);
        return new(name, ent, null);
    }

    private LookUpInEntity AppSettings(int appId)
    {
        var values = new Dictionary<string, object>
        {
            {Attributes.TitleNiceName, "App Settings"},
            {"DefaultCategoryName", LookUpEngineTests.DefaultCategory},
            {"MaxPictures", LookUpEngineTests.MaxPictures},
            {"PicsPerRow", "3"}
        };
        return BuildLookUpEntity(KeyAppSettings, values, appId);
    }

    private LookUpInEntity AppResources(int appId)
    {
        var values = new Dictionary<string, object>
        {
            {Attributes.TitleNiceName, "Resources"},
            {"Greeting", "Hello there!"},
            {"Introduction", "Welcome to this"}
        };
        return BuildLookUpEntity(KeyAppResources, values, appId);
    }
}