using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Core.Tests.Data;

public class SampleData(DataBuilder builder)
{
    //public const int AppId = -1;

    //private IContentType CtTestType => builder.ContentType.CreateContentTypeTac(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
    //    {
    //        builder.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0),
    //        builder.ContentTypeAttributeTac(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
    //    }
    //);


    //private IContentType CtPet => builder.ContentType.CreateContentTypeTac(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
    //    {
    //        builder.ContentTypeAttributeTac(AppId, "FirstName", DataTypes.String, true, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "LastName", DataTypes.String, false, 0, 0),
    //        //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "Phone", DataTypes.String, false, 0, 0),
    //        builder.ContentTypeAttributeTac(AppId, "Age", DataTypes.Number, false, 0,0)
    //    }
    //);

    //public IEntity TestEntityDaniel()
    //{
    //    var valDaniel = new Dictionary<string, object>
    //    {
    //        {"FirstName", "Daniel"},
    //        {"LastName", "Mettler"},
    //        {"Phone", "+41 81 750 67 70"},
    //        {"Age", 37},
    //        {"AnyDate", DateTime.Parse("2019-11-06T01:00:05Z") }
    //    };
    //    var entDaniel = builder.CreateEntityTac(appId: AppId, entityId: 1, contentType: CtTestType, values: valDaniel, titleField: "FirstName");
    //    return entDaniel;
    //}

    //public IEntity TestEntityLeonie()
    //{
    //    var valLeonie = new Dictionary<string, object>
    //    {
    //        {"FirstName", "Leonie"},
    //        {"LastName", "Mettler"},
    //        {"Phone", "+41 81 xxx yy zz"},
    //        {"Age", 6}
    //    };

    //    var entLeonie = builder.CreateEntityTac(appId: AppId, entityId: 2, contentType: CtTestType, values: valLeonie, titleField: "FirstName");
    //    return entLeonie;
    //}
    //public IEntity TestEntityPet(int petNumber)
    //{
    //    var valsPet = new Dictionary<string, object>
    //    {
    //        {"FirstName", "PetNo" + petNumber},
    //        {"LastName", "Of Bonsaikitten"},
    //        {"Phone", "+41 81 xxx yy zz"},
    //        {"Age", petNumber}
    //    };

    //    var entPet = builder.CreateEntityTac(appId: AppId, entityId: 1000 + petNumber, contentType: CtPet, values: valsPet, titleField: "FirstName");
    //    return entPet;
    //}
        

}