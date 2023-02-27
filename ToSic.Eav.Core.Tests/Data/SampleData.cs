using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Core.Tests.Data
{
    public class SampleData
    {
        private readonly MultiBuilder _builder;
        public const int AppId = -1;

        public SampleData(MultiBuilder builder)
        {
            _builder = builder;
        }

        static ContentTypeAttribute ContentTypeAttribute(int appId, string firstName, string dataType, bool isTitle, int attId, int index)
        {
            return new ContentTypeAttribute(appId, firstName, dataType, isTitle, attId, index);
        }

        IContentType CtTestType => _builder.ContentType.TestCreate(appId: AppId, name: "TestType", attributes: new List<IContentTypeAttribute>
            {
                ContentTypeAttribute(AppId, "FirstName", DataTypes.String, true, 0, 0),
                ContentTypeAttribute(AppId, "LastName", DataTypes.String, false, 0, 0),
                ContentTypeAttribute(AppId, "Phone", DataTypes.String, false, 0, 0),
                ContentTypeAttribute(AppId, "Age", DataTypes.Number, false, 0,0),
                ContentTypeAttribute(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
            }
        );


        IContentType CtPet => _builder.ContentType.TestCreate(appId: AppId, name: "Pet", attributes: new List<IContentTypeAttribute>
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
            var entDaniel = _builder.Entity.TestCreate(appId: AppId, entityId: 1, contentType: CtTestType, values: valDaniel, titleField: "FirstName");
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

            var entLeonie = _builder.Entity.TestCreate(appId: AppId, entityId: 2, contentType: CtTestType, values: valLeonie, titleField: "FirstName");
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

            var entPet = _builder.Entity.TestCreate(appId: AppId, entityId: 1000 + petNumber, contentType: CtPet, values: valsPet, titleField: "FirstName");
            return entPet;
        }
        

    }
}
