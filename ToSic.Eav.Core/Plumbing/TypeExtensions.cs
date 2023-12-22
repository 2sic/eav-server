﻿using System;
using System.Linq;

namespace ToSic.Eav.Plumbing;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class TypeExtensions
{
    /// <summary>
    /// If it's a nullable type (like int?) then get the unboxed type (like int)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static Type UnboxIfNullable(this Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(t) ?? t
            : t;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsNumeric(this object o) => !(o is null) && o.GetType().IsNumeric();

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsNumeric(this Type t)
    {
        switch (Type.GetTypeCode(t))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static T GetDirectlyAttachedAttribute<T>(this Type type) where T : class
        => type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
}