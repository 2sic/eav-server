﻿#if NETCOREAPP
// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

#pragma warning disable 219, 612, 618
#nullable disable

namespace ToSic.Eav.Persistence.Efc.Models
{
    internal partial class ToSicEavValuesDimensionsEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "ToSic.Eav.Persistence.Efc.Models.ToSicEavValuesDimensions",
                typeof(ToSicEavValuesDimensions),
                baseEntityType);

            var valueId = runtimeEntityType.AddProperty(
                "ValueId",
                typeof(int),
                propertyInfo: typeof(ToSicEavValuesDimensions).GetProperty("ValueId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValuesDimensions).GetField("<ValueId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0);
            valueId.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v));
            valueId.AddAnnotation("Relational:ColumnName", "ValueID");
            valueId.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var dimensionId = runtimeEntityType.AddProperty(
                "DimensionId",
                typeof(int),
                propertyInfo: typeof(ToSicEavValuesDimensions).GetProperty("DimensionId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValuesDimensions).GetField("<DimensionId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0);
            dimensionId.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v));
            dimensionId.AddAnnotation("Relational:ColumnName", "DimensionID");
            dimensionId.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var readOnly = runtimeEntityType.AddProperty(
                "ReadOnly",
                typeof(bool),
                propertyInfo: typeof(ToSicEavValuesDimensions).GetProperty("ReadOnly", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValuesDimensions).GetField("<ReadOnly>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: false);
            readOnly.TypeMapping = SqlServerBoolTypeMapping.Default.Clone(
                comparer: new ValueComparer<bool>(
                    (bool v1, bool v2) => v1 == v2,
                    (bool v) => v.GetHashCode(),
                    (bool v) => v),
                keyComparer: new ValueComparer<bool>(
                    (bool v1, bool v2) => v1 == v2,
                    (bool v) => v.GetHashCode(),
                    (bool v) => v),
                providerValueComparer: new ValueComparer<bool>(
                    (bool v1, bool v2) => v1 == v2,
                    (bool v) => v.GetHashCode(),
                    (bool v) => v));
            readOnly.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var key = runtimeEntityType.AddKey(
                new[] { valueId, dimensionId });
            runtimeEntityType.SetPrimaryKey(key);
            key.AddAnnotation("Relational:Name", "PK_ToSIC_EAV_ValuesDimensions");

            var index = runtimeEntityType.AddIndex(
                new[] { dimensionId });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("DimensionId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("DimensionId") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var dimension = declaringEntityType.AddNavigation("Dimension",
                runtimeForeignKey,
                onDependent: true,
                typeof(ToSicEavDimensions),
                propertyInfo: typeof(ToSicEavValuesDimensions).GetProperty("Dimension", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValuesDimensions).GetField("<Dimension>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var toSicEavValuesDimensions = principalEntityType.AddNavigation("ToSicEavValuesDimensions",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ToSicEavValuesDimensions>),
                propertyInfo: typeof(ToSicEavDimensions).GetProperty("ToSicEavValuesDimensions", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavDimensions).GetField("<ToSicEavValuesDimensions>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ToSIC_EAV_ValuesDimensions_ToSIC_EAV_Dimensions");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ValueId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ValueId") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var value = declaringEntityType.AddNavigation("Value",
                runtimeForeignKey,
                onDependent: true,
                typeof(ToSicEavValues),
                propertyInfo: typeof(ToSicEavValuesDimensions).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValuesDimensions).GetField("<Value>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var toSicEavValuesDimensions = principalEntityType.AddNavigation("ToSicEavValuesDimensions",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ToSicEavValuesDimensions>),
                propertyInfo: typeof(ToSicEavValues).GetProperty("ToSicEavValuesDimensions", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ToSicEavValues).GetField("<ToSicEavValuesDimensions>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ToSIC_EAV_ValuesDimensions_ToSIC_EAV_Values");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ToSIC_EAV_ValuesDimensions");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
#endif
