using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ToSic.Eav.BLL;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.DataSources.SqlSources;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Testing
{
	[TestFixture]
	class EAVContextTest
	{
		[Test]
		public void TestCache()
		{
			var Context = new EavContext();
			//Context.ItemCachingEnabled = true;

			for (int i = 0; i < 10000; i++)
			{
				//Context.GetItems(new List<int>() { 274, 275, 276 });
				//Context.GetItems(new List<int>() { 273 });
				//Context.GetItems(new List<int>() { 272 });
			}

			Context.SaveChanges();
		}

		[Test]
		public void DimensionsTest()
		{
			var Context = new EavContext();
		    var DbS = new DbShortcuts(Context);

			var Entity = DbS.GetEntity(277);

			//Context.UpdateEntity();

		}

		[Test]
		public void DimensionsCacheTest()
		{
			var Context = new EavContext();
		    var dim = Context.DimCommands;
			var Entity1 = dim.GetLanguages();
			var Entity2 = dim.GetLanguages();
			var Entity3 = dim.GetLanguages();

			//Context.UpdateEntity();

		}

		[Test]
		public void TestGetEntity()
		{
			//var Context = new EavContext();
			//IEntity entity = Context.GetEntityModel(303);

			//var notesAttrib = (IAttribute<string>)entity["Notes"];
			////notesAttrib.TypedContents


			//IAttribute firstNameAttrib = entity["FirstName"];
			//var firstName = (IAttribute<string>)firstNameAttrib;

			//IAttribute ageAttrib = entity["Age"];
			////var age = (IAttribute<int?>)ageAttrib;
			////ageAttrib[0];

			//var firstNameContents = firstName.TypedContents;
		}

		[Test]
		public void TestInitialDataSource()
		{
			var dsrc = DataSource.GetInitialDataSource();
			//var allEntities = dsrc.GetEntities(null);
			//Assert.IsNotNull(allEntities);
		}

		[Test]
		public void UpdateWithVersioning()
		{
			var context = EavContext.Instance(1, 1);
			var entityId = 280;
			var userName = "Testing 2bg 17:51";
			context.UserName = userName;
			var newValues = new Dictionary<string, ValueToImport>
				{
					{"FirstName", new ValueToImport {Value = "Benjamin 17:51"}},
					{"LastName", new ValueToImport {Value = "Gemperle 17:51"}},
					{"Address", new ValueToImport {Value = "Churerstrasse 35 17:51"}},
					{"ZIP", new ValueToImport {Value = "9470 17:51"}},
					{"City", new ValueToImport {Value = "Buchs 17:51"}}
				};

			context.EntCommands.UpdateEntity(entityId, newValues);
		}

		[Test]
		public void AddEntity()
		{
			var context = EavContext.Instance(1, 1);
			var userName = "Testing 2bg 17:53";
			context.UserName = userName;
			var newValues = new Dictionary<string, ValueToImport>
				{
					{"FirstName", new ValueToImport {Value = "Benjamin 17:51"}},
					{"LastName", new ValueToImport {Value = "Gemperle 17:51"}},
					{"Address", new ValueToImport {Value = "Churerstrasse 35 17:51"}},
					{"ZIP", new ValueToImport {Value = "9470 17:51"}},
					{"City", new ValueToImport {Value = "Buchs 17:51"}}
				};

			context.EntCommands.AddEntity(37, newValues, null, null);
		}

		[Test]
		public void EntityRelationshipUpdate()
		{
			var context = EavContext.Instance(1, 1);
			var entityId = 2372;
			var newValues = new Dictionary<string, ValueToImport>();
			context.EntCommands.UpdateEntity(entityId, newValues);
		}

		[Test]
		public void GetDataForCache()
		{
			var sqlStore = new EavSqlStore();
			var cache = new QuickCache { AppId = 1, ZoneId = 1 };
			sqlStore.GetDataForCache(cache);
		}

		[Test]
		public void GetDataForCache2()
		{
			var db = new EavContext();

			var appId = 1;
			var entitiesValues = from e in db.Entities
								 where !e.ChangeLogIDDeleted.HasValue && e.Set.AppID == appId
								 select new
								 {
									 e.EntityID,
									 e.EntityGUID,
									 e.AttributeSetID,
									 e.KeyGuid,
									 e.KeyNumber,
									 e.KeyString,
									 e.AssignmentObjectTypeID,
									 RelatedEntities = from r in e.EntityParentRelationships
													   group r by r.AttributeID into rg
													   select new
													   {
														   AttributeID = rg.Key,
														   AttributeName = rg.Select(a => a.Attribute.StaticName).FirstOrDefault(),
														   IsTitle = rg.Any(v1 => v1.Attribute.AttributesInSets.Any(s => s.IsTitle)),
														   Childs = rg.OrderBy(c => c.SortOrder).Select(c => c.ChildEntityID)
													   },
									 Attributes = from v in e.Values
												  where !v.ChangeLogIDDeleted.HasValue
												  group v by v.AttributeID into vg
												  select new
												  {
													  AttributeID = vg.Key,
													  AttributeName = vg.Select(v1 => v1.Attribute.StaticName).FirstOrDefault(),
													  AttributeType = vg.Select(v1 => v1.Attribute.Type).FirstOrDefault(),
													  IsTitle = vg.Any(v1 => v1.Attribute.AttributesInSets.Any(s => s.IsTitle)),
													  Values = from v2 in vg
															   orderby v2.ChangeLogIDCreated
															   select new
															   {
																   v2.ValueID,
																   v2.Value,
																   Languages = from l in v2.ValuesDimensions select new { DimensionId = l.DimensionID, ReadOnly = l.ReadOnly, Key = l.Dimension.ExternalKey },
																   v2.ChangeLogIDCreated
															   }
												  }
								 };
			entitiesValues.ToList();
		}

		[Test]
		public void EnsureSharedAttributeSets()
		{
			var db = EavContext.Instance(appId: 2);
			//foreach (var app in db.Apps)
			//	db.EnsureSharedAttributeSets(app.);

			db.SaveChanges();
		}

		/// <summary>
		/// Add an App with Data and remove it again completely
		/// </summary>
		[Test]
		public void AddRemoveApp()
		{
			var db = EavContext.Instance();
		    var DbS = new DbShortcuts(db);
			// Add new App
			var app = DbS.AddApp("Test Clean Remove");
			db.AppId = app.AppID;

			// Add new AttributeSet
			var attributeSet = db.AttSetCommands.AddAttributeSet("Sample Attribute Set", "Sample Attribute Set Description", null, "");
			db.AttrCommands.AppendAttribute(attributeSet, "Attribute1", "String", true);
			var attribute2 = db.AttrCommands.AppendAttribute(attributeSet, "Attribute2", "String");
			var attribute3 = db.AttrCommands.AppendAttribute(attributeSet, "Attribute3", "String");
			var attribute4 = db.AttrCommands.AppendAttribute(attributeSet, "Attribute4", "Entity");

			// Add new Entities
			var values = new Dictionary<string, ValueToImport>
			{
				{"Attribute1", new ValueToImport{ Value = "Sample Value 1"}},
				{"Attribute2", new ValueToImport{ Value = "Sample Value 2"}},
				{"Attribute3", new ValueToImport{ Value = "Sample Value 3"}},
			};
			var entity1 = db.EntCommands.AddEntity(attributeSet, values, null, null, dimensionIds: new[] { 2 });
			values.Add("Attribute4", new ValueToImport { Value = new[] { entity1.EntityID } });
			var entity2 = db.EntCommands.AddEntity(attributeSet, values, null, null, dimensionIds: new[] { 2 });

			// Update existing Entity
			values["Attribute3"].Value = "Sample Value 3 modified";
			db.EntCommands.UpdateEntity(entity1.EntityID, values, dimensionIds: new[] { 2 });

			// update existing AttributeSets
			db.AttrCommands.UpdateAttribute(attribute2.AttributeID, "Attribute2Renamed");
			db.AttSetCommands.RemoveAttributeInSet(attribute3.AttributeID, attributeSet.AttributeSetID);

			// Delete the App
			DbS.DeleteApp(app.AppID);
		}

		//[Test]
		//public void CloneEntity()
		//{
		//	var db = EavContext.Instance(appId: 2);
		//	var sourceEntity = db.GetEntity(330);
		//	var clone = db.CloneEntity(sourceEntity);
		//	clone.IsPublished = false;
		//	db.AddToEntities(clone);

		//	db.SaveChanges();
		//}

		[Test]
		public void DraftEntitiesTest()
		{
			var db1 = EavContext.Instance(appId: 2);
			var publishedWitDraft = new DbLoadIntoEavDataStructure(db1).GetEavEntity(5454);
			Assert.NotNull(publishedWitDraft.GetDraft());

			var db2 = EavContext.Instance(appId: 2);
            var draftEntity = new DbLoadIntoEavDataStructure(db2).GetEavEntity(5458);
			Assert.NotNull(draftEntity.GetPublished());
		}

		[Test]
		public void GetEntityModel()
		{
			var ctx = EavContext.Instance(appId: 1);
			var entityIds = new[] { 45 };
			foreach (var entityId in entityIds)
			{
                new DbLoadIntoEavDataStructure(ctx).GetEavEntity(entityId);
			}
		}
	}
}