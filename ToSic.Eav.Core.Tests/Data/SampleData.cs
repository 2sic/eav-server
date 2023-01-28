using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Core.Tests.Data
{
    public static class SampleData
    {
        public const int AppId = -1;

        static ContentTypeAttribute ContentTypeAttribute(int appId, string firstName, string dataType, bool isTitle, int attId, int index)
        {
            return new ContentTypeAttribute(appId, firstName, dataType, isTitle, attId, index);
        }

        static readonly ContentType CtTestType = new ContentType(AppId, "TestType")
        {
            Attributes = new List<IContentTypeAttribute>
            {
                ContentTypeAttribute(AppId, "FirstName", DataTypes.String, true, 0, 0),
                ContentTypeAttribute(AppId, "LastName", DataTypes.String, false, 0, 0),
                ContentTypeAttribute(AppId, "Phone", DataTypes.String, false, 0, 0),
                ContentTypeAttribute(AppId, "Age", DataTypes.Number, false, 0,0),
                ContentTypeAttribute(AppId, "AnyDate", DataTypes.DateTime, false, 0,0)
            }
        };


        static readonly ContentType CtPet = new ContentType(AppId, "Pet")
        {
            Attributes = new List<IContentTypeAttribute>
            {
                ContentTypeAttribute(AppId, "FirstName", DataTypes.String, true, 0, 0),
                ContentTypeAttribute(AppId, "LastName", DataTypes.String, false, 0, 0),
                //ContentTypeAttribute(AppId, "Birthday", "DateTime", true, 0, 0),
                ContentTypeAttribute(AppId, "Phone", DataTypes.String, false, 0, 0),
                ContentTypeAttribute(AppId, "Age", DataTypes.Number, false, 0,0)
            }
        };

        public static IEntity TestEntityDaniel()
        {
            var valDaniel = new Dictionary<string, object>()
            {
                {"FirstName", "Daniel"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 750 67 70"},
                {"Age", 37},
                {"AnyDate", DateTime.Parse("2019-11-06T01:00:05Z") }
            };
            var entDaniel = new Entity(AppId, 1, CtTestType, valDaniel, "FirstName");
            return entDaniel;
        }

        public static IEntity TestEntityLeonie()
        {
            var valLeonie = new Dictionary<string, object>()
            {
                {"FirstName", "Leonie"},
                {"LastName", "Mettler"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", 6}
            };

            var entLeonie = new Entity(AppId, 2, CtTestType, valLeonie, "FirstName");
            return entLeonie;
        }
        public static IEntity TestEntityPet(int petNumber)
        {
            var valsPet = new Dictionary<string, object>()
            {
                {"FirstName", "PetNo" + petNumber},
                {"LastName", "Of Bonsaikitten"},
                {"Phone", "+41 81 xxx yy zz"},
                {"Age", petNumber}
            };

            var entPet = new Entity(AppId, 1000 + petNumber, CtPet, valsPet, "FirstName");
            return entPet;
        }
        

    }
}
