using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Core.Tests.Data;

public class SampleData(DataBuilder builder)
{
    public const int AppId = -1;


    public static ContentTypeAttribute ContentTypeAttribute(DataBuilder builder, int appId, string name, string dataType, bool isTitle = false, int attId = 0, int index = 0)
        => builder.TypeAttributeBuilder.Create(appId: appId, name: name, type: ValueTypeHelpers.Get(dataType), isTitle: isTitle, id: attId, sortOrder: index);

    private ContentTypeAttribute ContentTypeAttribute(int appId, string firstName, string dataType, bool isTitle, int attId, int index)
    {
        return builder.TypeAttributeBuilder.Create(appId: appId, name: firstName, type: ValueTypeHelpers.Get(dataType), isTitle: isTitle, id: attId, sortOrder: index);
    }

    IContentType CtTestType => builder.ContentType.TestCreate(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
        {
            ContentTypeAttribute(AppId, "FirstName", DataTypes.String, true, 0, 0),
            ContentTypeAttribute(AppId, "LastName", DataTypes.String, false, 0, 0),
            ContentTypeAttribute(AppId, "Phone", DataTypes.String, false, 0, 0),
            ContentTypeAttribute(AppId, "Age", DataTypes.Number, false, 0,0),
            ContentTypeAttribute(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
        }
    );


    IContentType CtPet => builder.ContentType.TestCreate(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
        {
            ContentTypeAttribute(AppId, "FirstName", DataTypes.String, true, 0, 0),
            ContentTypeAttribute(AppId, "LastName", DataTypes.String, false, 0, 0),
            //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
            ContentTypeAttribute(AppId, "Phone", DataTypes.String, false, 0, 0),
            ContentTypeAttribute(AppId, "Age", DataTypes.Number, false, 0,0)
        }
    );

    public IEntity TestEntityDaniel()
    {
        var valDaniel = new Dictionary<string, object>
        {
            {"FirstName", "Daniel"},
            {"LastName", "Mettler"},
            {"Phone", "+41 81 750 67 70"},
            {"Age", 37},
            {"AnyDate", DateTime.Parse("2019-11-06T01:00:05Z") }
        };
        var entDaniel = builder.TestCreate(appId: AppId, entityId: 1, contentType: CtTestType, values: valDaniel, titleField: "FirstName");
        return entDaniel;
    }

    public IEntity TestEntityLeonie()
    {
        var valLeonie = new Dictionary<string, object>
        {
            {"FirstName", "Leonie"},
            {"LastName", "Mettler"},
            {"Phone", "+41 81 xxx yy zz"},
            {"Age", 6}
        };

        var entLeonie = builder.TestCreate(appId: AppId, entityId: 2, contentType: CtTestType, values: valLeonie, titleField: "FirstName");
        return entLeonie;
    }
    public IEntity TestEntityPet(int petNumber)
    {
        var valsPet = new Dictionary<string, object>
        {
            {"FirstName", "PetNo" + petNumber},
            {"LastName", "Of Bonsaikitten"},
            {"Phone", "+41 81 xxx yy zz"},
            {"Age", petNumber}
        };

        var entPet = builder.TestCreate(appId: AppId, entityId: 1000 + petNumber, contentType: CtPet, values: valsPet, titleField: "FirstName");
        return entPet;
    }
        

}