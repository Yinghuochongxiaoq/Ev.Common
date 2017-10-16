/**============================================================
* 命名空间: Ev.Common.SqlHelper
*
* 功 能： N/A
* 类 名： SetPropertyModel
*
* Ver 变更日期 负责人 变更内容
* ───────────────────────────────────
* V0.01 2017/10/16 14:58:31 FreshMan 初版
*
* Copyright (c) 2017 Lir Corporation. All rights reserved.
*==============================================================
*==此技术信息为本公司机密信息,未经本公司书面同意禁止向第三方披露==
*==版权所有：重庆慧都科技有限公司                             ==
*==============================================================
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DevExpress.Xpo;

namespace Ev.Common.SqlHelper
{
    /// <summary>
    /// 设置属性值的委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="obj1"></param>
    /// <returns></returns>
    internal delegate T SetPropertyValueInvoker<T>(T obj, IDataReader obj1);

    /// <summary>
    /// 获取属性的值到DataRow中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="dataRow"></param>
    /// <returns></returns>
    internal delegate DataRow GetPropertyValueInvoker<T>(T obj, DataRow dataRow);

    /// <summary>
    /// 设置属性值方法
    /// </summary>
    internal static class SetPropertyModel<T>
    {
        /// <summary>
        /// 类型数据
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentBag<PropertyInfo> WriteProperties;

        /// <summary>
        /// 构造函数
        /// </summary>
        static SetPropertyModel()
        {
            var typeOfT = typeof(T);
            WriteProperties = SimpleCrud.GetWriteProperties(typeOfT);
        }

        /// <summary>
        /// 获取设置的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static T SetPropertyValue(T obj, IDataReader dataReader)
        {
            if (dataReader == null) return obj;
            foreach (var tempProperty in WriteProperties)
            {
                var value = SecureReaderGetValue(dataReader, tempProperty.Name, tempProperty.PropertyType);
                tempProperty.SetValue(obj, value, null);
            }
            return obj;
        }

        /// <summary>
        /// 获取单个属性的值
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object SecureReaderGetValue(IDataReader dataReader, string name, Type type)
        {
            if (dataReader == null || string.IsNullOrEmpty(name)) return null;
            var schemaTable = dataReader.GetSchemaTable();
            if (schemaTable == null) return null;
            schemaTable.DefaultView.RowFilter = "ColumnName= '" + name + "'";
            if (schemaTable.DefaultView.Count < 1) return null;
            var obj = dataReader[name];
            if (typeof(Enum).IsAssignableFrom(type))
            {
                return Enum.Parse(type, obj.ToString());
            }
            return obj;
        }
    }

    /// <summary>
    /// 获取属性值到DataRow中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class GetPropertyModel<T>
    {
        /// <summary>
        /// 类型数据
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentBag<PropertyInfo> ReadProperties;

        /// <summary>
        /// 构造函数
        /// </summary>
        static GetPropertyModel()
        {
            var typeOfT = typeof(T);
            ReadProperties = SimpleCrud.GetProperties(typeOfT);
        }

        /// <summary>
        /// 获取设置的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static DataRow GetPropertyValue(T obj, DataRow row)
        {
            if (row == null) return null;
            foreach (var property in ReadProperties)
            {
                row[property.Name] = SimpleCrud.GetReflectorValue(property, obj);
            }
            return row;
        }
    }
}
