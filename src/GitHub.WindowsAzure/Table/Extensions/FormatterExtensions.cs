﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace GitHub.WindowsAzure.Table.Extensions
{
    public static class FormatterExtensions
    {
        private static readonly Type DateTimeType = typeof (DateTime);

        private static readonly Dictionary<Type, Func<object, EntityProperty>> TypeToEdm =
            new Dictionary<Type, Func<object, EntityProperty>>
                {
                    {typeof (string), o => new EntityProperty((string) o)},
                    {typeof (byte[]), o => new EntityProperty((byte[]) o)},
                    {typeof (bool), o => new EntityProperty((bool) o)},
                    {typeof (DateTime), o => new EntityProperty((DateTime) o)},
                    {typeof (DateTimeOffset), o => new EntityProperty((DateTimeOffset) o)},
                    {typeof (double), o => new EntityProperty((double) o)},
                    {typeof (Guid), o => new EntityProperty((Guid) o)},
                    {typeof (int), o => new EntityProperty((int) o)},
                    {typeof (long), o => new EntityProperty((long) o)}
                };

        private static readonly Dictionary<EdmType, Action<PropertyInfo, EntityProperty, object>> EdmToType =
            new Dictionary<EdmType, Action<PropertyInfo, EntityProperty, object>>
                {
                    {EdmType.Binary, (p, e, t) => p.SetValue(t, e.BinaryValue)},
                    {EdmType.Boolean, (p, e, t) => p.SetValue(t, e.BooleanValue)},
                    {EdmType.Double, (p, e, t) => p.SetValue(t, e.DoubleValue)},
                    {EdmType.Guid, (p, e, t) => p.SetValue(t, e.GuidValue)},
                    {EdmType.Int32, (p, e, t) => p.SetValue(t, e.Int32Value)},
                    {EdmType.Int64, (p, e, t) => p.SetValue(t, e.Int64Value)},
                    {EdmType.String, (p, e, t) => p.SetValue(t, e.StringValue)},
                    {
                        EdmType.DateTime,
                        (p, e, t) =>
                            {
                                if (p.PropertyType == DateTimeType)
                                {
                                    if (e.DateTimeOffsetValue != null)
                                    {
                                        p.SetValue(t, e.DateTimeOffsetValue.Value.DateTime);
                                    }
                                }
                                else
                                {
                                    p.SetValue(t, e.DateTimeOffsetValue);
                                }
                            }
                    },
                };

        public static EntityProperty GetStringValue(this PropertyInfo property, object target)
        {
            var value = (string) property.GetValue(target);

            return new EntityProperty(value);
        }

        public static EntityProperty GetEntityProperty(this PropertyInfo property, object target)
        {
            object value = property.GetValue(target);

            if (!TypeToEdm.ContainsKey(property.PropertyType))
            {
                throw new Exception(string.Format("Invalid property type: {0}", property.Name));
            }

            return TypeToEdm[property.PropertyType](value);
        }

        public static void SetPropertyValue(this PropertyInfo property, EntityProperty entityProperty, object target)
        {
            if (!EdmToType.ContainsKey(entityProperty.PropertyType))
            {
                throw new Exception("Invalid entity property EDM type.");
            }

            EdmToType[entityProperty.PropertyType](property, entityProperty, target);
        }
    }
}