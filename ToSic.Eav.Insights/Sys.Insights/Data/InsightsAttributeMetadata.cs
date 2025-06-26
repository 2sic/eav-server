﻿using ToSic.Eav.WebApi.Sys.Helpers.Http;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Data;

internal class InsightsAttributeMetadata(LazySvc<IAppReaderFactory> appReaders, IHttpExceptionMaker exceptionMaker)
    : InsightsProvider(new() { Name = Link, Title = "Attribute Metadata "}, connect: [appReaders])
{
    public static string Link = "AttributeMetadata";

    public override string HtmlBody()
    {
        if (UrlParamsIncomplete(AppId, Type, NameId, out var message))
            return message;

        Log.A($"debug app metadata for {AppId} and {Type}");
        var typ = appReaders.Value.Get(AppId.Value).GetContentType(Type);
        var att = typ.Attributes.First(a => a.Name == NameId)
                  ?? throw exceptionMaker.BadRequest($"can't find attribute {NameId}");

        var msg = H1($"Attribute Metadata for {typ.Name}.{NameId} in {AppId}\n").ToString();
        var metadata = att.Metadata.ToList();

        return MetadataHelper.MetadataTable(msg, AppId.Value, metadata, Linker);
    }

}